using System;
using System.Collections.Generic;
using System.Linq;

namespace Player3
{
    class Game
    {
        List<Zone> zones; // all game zones
        List<Team> teams; // all the team of drones. Array index = team's ID
        int myTeamId; // index of my team in the array of teams

        // Compute logic here. This method is called for each game round. 
        public void Play()
        {
            CalculateDronesOnZone();

            CalculateZoneValues();

            CalculateDronesWhoCantMove();

            var orderedZones = zones.OrderByDescending(x => x.Value).ToArray();

            //foreach (var orderedZone in orderedZones)
            //{
            //    var droneDistances = GetNearestDrones(orderedZone);
            //    if (droneDistances != null)
            //    {
            //        foreach (var droneDistance in droneDistances)
            //        {
            //            var drone = droneDistance.Item1;
            //            drone.NextMove = orderedZone.Center;
            //            Console.Error.WriteLine("Drone {0} va capturer zone {1}", teams[myTeamId].Drones.IndexOf(drone), zones.IndexOf(orderedZone));
            //        }
            //    }
            //}

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 200);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 400);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 700);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 1000);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 2000);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 3000);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            foreach (var orderedZone in orderedZones)
            {
                CaptureZoneInRange(orderedZone, 4000);
            }
            teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("Drone {0} disponible", teams[myTeamId].Drones.IndexOf(x)));

            //foreach (var orderedZone in orderedZones)
            //{
            //    var drone = teams[myTeamId].Drones.FirstOrDefault(x => x.NextMove == null);
            //    if (drone != null)
            //    {
            //        drone.NextMove = orderedZone.Center;
            //        teams[myTeamId].Drones.Where(x => x.NextMove == null).ToList().ForEach(x => Console.Error.WriteLine("A défaut, on envoie le drone {0} sur la zone {1}", teams[myTeamId].Drones.IndexOf(x), zones.IndexOf(orderedZone)));
            //    }
            //}
            foreach (var myDrone in teams[myTeamId].Drones)
            {
                Console.WriteLine(myDrone.NextMove ?? myDrone.Position);
                myDrone.NextMove = null;
            }
        }

        private void CalculateDronesWhoCantMove()
        {
            foreach (var zone in zones.Where(x => x.OwnerId == myTeamId))
            {
                foreach (var drone in zone.DronesOnZone.Where(x => x.Team.TeamId == myTeamId).Take(zone.MaxEnnemyDroneCount))
                {
                    drone.NextMove = zone.Center;
                    Console.Error.WriteLine("Drone {0} reste sécuriser zone {1}", teams[myTeamId].Drones.IndexOf(drone), zones.IndexOf(zone));
                }
            }
        }

        private void CalculateDronesOnZone()
        {
            foreach (var zone in zones)
            {
                zone.DronesOnZone.Clear();
                zone.MaxEnnemyDroneCount = 0;
                zone.MyDroneCount = 0;
                foreach (var team in teams)
                {
                    var droneCount = 0;
                    foreach (var drone in team.Drones)
                    {
                        if ((Math.Abs(drone.Position.X - zone.Center.X) <= Zone.Radius && Math.Abs(drone.Position.Y - zone.Center.Y) <= Zone.Radius))
                        {
                            droneCount++;
                            zone.DronesOnZone.Add(drone);
                        }
                    }

                    if (team.TeamId == myTeamId)
                    {
                        zone.MyDroneCount = droneCount;
                    }
                    else
                    {
                        if (droneCount > zone.MaxEnnemyDroneCount)
                        {
                            zone.MaxEnnemyDroneCount = droneCount;
                        }
                    }
                }

                Console.Error.WriteLine("Zone {0}, Nombre de drones : {1}, Ami : {2}, Max ennemi : {3}", zones.IndexOf(zone), zone.DronesOnZone.Count, zone.MyDroneCount, zone.MaxEnnemyDroneCount);
            }
        }

        private IEnumerable<Tuple<Drone, double>> GetNearestDrones(Zone zone)
        {
            var droneDistance = (from myDrone in teams[myTeamId].Drones
                                 where myDrone.NextMove == null // Sauf les drones qui sécurisent une zone
                                 let newDistance = Distance(zone.Center, myDrone.Position)
                                 select new Tuple<Drone, double>(myDrone, newDistance)
                                ).ToList();

            //foreach (var tuple in droneDistance)
            //{
            //    Console.Error.WriteLine("Distance du drone {0} à la zone {1} : {2}", teams[myTeamId].Drones.IndexOf(tuple.Item1),
            //                            zones.IndexOf(zone), tuple.Item2);
            //}

            // Si on a assez de drones disponibles, on les envoie
            var dronesNeeded = (zone.MaxEnnemyDroneCount + 1); // On n'enlève pas les drones déjà sur zone au calcul car il faut qu'ils aient également l'ordre de déplacement
            if (droneDistance.Count >= dronesNeeded)
            {
                return droneDistance.OrderBy(x => x.Item2).Take(dronesNeeded);
            }
            return null;
        }

        private void CaptureZoneInRange(Zone zone, double range)
        {
            var dronesNeeded = 0;
            var myDrones = GetMyDronesInRange(zone, range).Where(x => x.Item1.NextMove == null ||
                                                                      x.Item1.NextMove.Equals(zone.Center)).ToList();
            var ennemies = GetMaxEnnemyDronesInRange(zone, range);

            dronesNeeded = ennemies;
            if (zone.OwnerId != myTeamId)
            {
                dronesNeeded = ennemies + 1;
            }

            Console.Error.WriteLine("Zone {0}, range {1}, ennemis : {2}, amis : {3}, need : {4}", zones.IndexOf(zone),
                                    range, ennemies, myDrones.Count, dronesNeeded);

            if (myDrones.Count() >= dronesNeeded)
            {
                myDrones.OrderBy(x => x.Item2).Take(dronesNeeded).ToList().ForEach(x =>
                {
                    if (x.Item1.NextMove != null)
                    {
                        Console.Error.WriteLine("Drone {0} reste sécuriser zone {1}",
                                                teams[myTeamId].Drones.IndexOf(x.Item1), zones.IndexOf(zone));
                    }
                    else
                    {
                        x.Item1.NextMove = zone.Center;
                        Console.Error.WriteLine("Drone {0} va capturer zone {1}",
                                                teams[myTeamId].Drones.IndexOf(x.Item1), zones.IndexOf(zone));
                    }
                });
            }
        }

        private IEnumerable<Tuple<Drone, double>> GetMyDronesInRange(Zone zone, double range)
        {
            var t = GetDronesInRange(zone, range).SingleOrDefault(x => x.Key.TeamId == myTeamId);

            return t != null ? t.ToList() : new List<Tuple<Drone, double>>(0);
        }

        private int GetMaxEnnemyDronesInRange(Zone zone, double range)
        {
            var max = 0;

            foreach (var group in GetDronesInRange(zone, range).Where(x => x.Key.TeamId != myTeamId))
            {
                if (group.Count() > max)
                {
                    max = group.Count();
                }
            }

            return max;
        }

        private List<IGrouping<Team, Tuple<Drone, double>>> GetDronesInRange(Zone zone, double range)
        {
            var droneDistance = (from team in teams
                                 where team.TeamId == myTeamId
                                 from myDrone in team.Drones
                                 let newDistance = Distance(zone.Center, myDrone.Position)
                                 where newDistance <= range
                                 group new Tuple<Drone, double>(myDrone, newDistance) by team
                                 ).ToList();

            var droneDistance2 = (from team in teams
                                  where team.TeamId != myTeamId
                                  from myDrone in team.Drones
                                  let newDistance = Distance(zone.Center, myDrone.Position)
                                  where newDistance <= 400
                                  group new Tuple<Drone, double>(myDrone, newDistance) by team
                                 ).ToList();

            //var droneDistance = (from team in teams
            //                     where team.TeamId <= myTeamId
            //                     from myDrone in team.Drones
            //                     let newDistance = Distance(zone.Center, myDrone.Position)
            //                     where newDistance <= range
            //                     group new Tuple<Drone, double>(myDrone, newDistance) by team
            //                     ).ToList();

            //var droneDistance2 = (from team in teams
            //                     where team.TeamId > myTeamId
            //                     from myDrone in team.Drones
            //                     let newDistance = Distance(zone.Center, myDrone.Position)
            //                     where newDistance <= range + Zone.Radius
            //                     group new Tuple<Drone, double>(myDrone, newDistance) by team
            //                     ).ToList();

            return droneDistance.Union(droneDistance2).ToList();
        }

        private void CalculateZoneValues()
        {
            foreach (var zone in zones)
            {
                int dronesNeeded;

                if (zone.OwnerId == -1)
                {
                    dronesNeeded = 1;
                    zone.Value = 100000;
                }
                else if (zone.OwnerId != myTeamId)
                {
                    dronesNeeded = (zone.MaxEnnemyDroneCount - zone.MyDroneCount + 1);
                    if (dronesNeeded <= 0)
                    {
                        dronesNeeded = 1;
                    }
                    zone.Value = 100000 / dronesNeeded; // Moins il faut de drones pour conquérir, plus ça a de la valeur
                }
                else
                {
                    dronesNeeded = 0;
                    zone.Value = 0;
                }

                // Les zones les plus proches ont plus de valeur
                var droneDistances = GetNearestDrones(zone);
                if (droneDistances != null)
                {
                    zone.Value -= droneDistances.Sum(x => x.Item2);
                }


                Console.Error.WriteLine("Zone {0}, Valeur : {1}, Coord : {2}", zones.IndexOf(zone), zone.Value, zone.Center);
            }
        }

        // read initial games data (one time at the beginning of the game: P I D Z...)
        public void Init()
        {
            var pidz = ReadIntegers();

            myTeamId = pidz[1];
            zones = ReadZones(pidz[3]).ToList();
            teams = ReadTeams(pidz[0], pidz[2]).ToList();
        }

        static IEnumerable<Zone> ReadZones(int zoneCount)
        {
            for (var areaId = 0; areaId < zoneCount; areaId++)
                yield return new Zone { Center = ReadPoint() };
        }

        static IEnumerable<Team> ReadTeams(int teamCount, int dronesPerTeam)
        {
            for (var teamId = 0; teamId < teamCount; teamId++)
                yield return new Team(teamId, dronesPerTeam);
        }

        public void ReadContext()
        {
            foreach (var zone in zones)
                zone.OwnerId = ReadIntegers()[0];

            foreach (var team in teams)
                foreach (var drone in team.Drones)
                    drone.Position = ReadPoint();
        }

        static public double Sqr(double a)
        {
            return a * a;
        }

        static public double Distance(Point start, Point end)
        {
            return Math.Sqrt(Sqr(end.Y - start.Y) + Sqr(end.X - start.X));
        }

        static int[] ReadIntegers()
        {
            // ReSharper disable PossibleNullReferenceException
            return Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
            // ReSharper restore PossibleNullReferenceException
        }

        static Point ReadPoint()
        {
            var xy = ReadIntegers();
            return new Point { X = xy[0], Y = xy[1] };
        }
    }

    static class Program
    {
        static void Main()
        {
            var game = new Game();

            game.Init();

            while (true)
            {
                game.ReadContext();
                game.Play();
            }
        }
    }

    class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            var other = (Point)obj;
            return X == other.X && Y == other.Y;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", X, Y);
        }
    }

    class Drone
    {
        public Drone(Team team)
        {
            Team = team;
        }

        public Team Team { get; private set; }
        public Point Position { get; set; }

        public Point NextMove { get; set; }
    }

    class Zone
    {
        public const int Radius = 100;

        public Point Center { get; set; }
        public int OwnerId { get; set; } // -1 if no owner
        public double Value { get; set; }
        public List<Drone> DronesOnZone = new List<Drone>();
        public int MaxEnnemyDroneCount { get; set; }
        public int MyDroneCount { get; set; }
    }

    class Team
    {
        public int TeamId { get; set; }

        public Team(int teamId, int droneCount)
        {
            TeamId = teamId;
            Drones = new List<Drone>(droneCount);
            for (var droneId = 0; droneId < droneCount; droneId++)
                Drones.Add(new Drone(this));
        }

        public IList<Drone> Drones { get; private set; }
    }

}