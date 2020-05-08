using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace SpringChallenge2020
{
    /**
 * Grab the pellets as fast as you can!
 **/
    public class Player
    {
        public static void Main(string[] args)
        {
            Game.Play();
        }

        public static class Game
        {
            public static void Play(string initData = null, string turnData = null)
            {
                var initInputProcessor = new InputProcessor(initData);
                var inputs = initInputProcessor.ReadLine().Split(' ');
                var map = new List<AStarSearch.Location>();
                int width = int.Parse(inputs[0]); // size of the grid
                int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
                for (int y = 0; y < height; y++)
                {
                    string row = initInputProcessor.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
                    for (var x = 0; x < width; x++)
                    {
                        map.Add(new AStarSearch.Location(new Position(x, y), row[x] == '#'));
                    }
                }

                var pacs = new List<Pac>();

                // game loop
                while (true)
                {
                    pacs.ForEach(_ => _.IsAlive = false);
                    var gameTurnInputProcessor = new InputProcessor(turnData);

                    inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                    int myScore = int.Parse(inputs[0]);
                    int opponentScore = int.Parse(inputs[1]);
                    int visiblePacCount = int.Parse(gameTurnInputProcessor.ReadLine()); // all your pacs and enemy pacs in sight
                    for (int i = 0; i < visiblePacCount; i++)
                    {
                        inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                        int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                        bool mine = inputs[1] != "0"; // true if this pac is yours
                        int x = int.Parse(inputs[2]); // position in the grid
                        int y = int.Parse(inputs[3]); // position in the grid
                        string typeId = inputs[4]; // unused in wood leagues
                        int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                        int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

                        if (!pacs.Any(_ => _.Id == pacId && _.IsMine == mine))
                            pacs.Add(new Pac { Id = pacId, IsMine = mine });
                        var pac = pacs.First(_ => _.Id == pacId && _.IsMine == mine);
                        pac.PreviousPosition = pac.Position;
                        pac.Position = new Position(x, y);
                        pac.PacType = Enum.Parse<PacType>(typeId);
                        pac.IsAlive = true;
                    }

                    pacs.RemoveAll(_ => !_.IsAlive);

                    var visiblePelletCount = int.Parse(gameTurnInputProcessor.ReadLine()); // all pellets in sight
                    var visiblePellets = new List<Pellet>();

                    for (int i = 0; i < visiblePelletCount; i++)
                    {
                        inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                        visiblePellets.Add(
                            new Pellet(int.Parse(inputs[0]), int.Parse(inputs[1]))
                            { Value = int.Parse(inputs[2]) }
                        );
                    }

                    initInputProcessor.WriteDebugData();
                    gameTurnInputProcessor.WriteDebugData();

                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");
                    foreach (var pac in pacs.Where(_ => _.IsMine))
                    {
                        // Try to eat opponent
                        //pacs.Single(e => e.Id == 1 && !e.IsMine).Position.Manhattan(pac.Position)
                        var opponent = pacs.FirstOrDefault(_ => _.Id != pac.Id && !_.IsMine && _.Position.Manhattan(pac.Position) <= 4);
                        if (opponent != null)
                        {
                            if (opponent.GetBetterType() == pac.PacType)
                            {
                                pac.Command = "MOVE";
                                pac.CommandArgs = opponent.Position;
                            }
                            else
                            {
                                pac.Command = "SWITCH";
                                pac.CommandArgs = opponent.GetBetterType();
                            }
                        }
                        else
                        {
                            var closest = visiblePellets
                                .Where(_ => !pacs.Where(p => p.IsMine).Select(p => p.CommandArgs)
                                    .Contains(_)) // Exclude destinations already set for other pacs
                                .OrderByDescending(_ => _.Value).ThenBy(_ => _.Manhattan(pac.Position))
                                .FirstOrDefault();

                            if (pac.Command == "MOVE" && pac.Position == pac.PreviousPosition)
                            {
                                // Had collision
                                var collidingPac = pacs.FirstOrDefault(_ =>
                                    _.Id != pac.Id && _.Position.Manhattan(pac.Position) <= 2);
                                if (collidingPac?.IsMine == true)
                                {
                                    pac.Command = "MOVE";
                                    pac.CommandArgs = new Position(0, 0);
                                }
                                else if (collidingPac?.IsMine == false)
                                {

                                    pac.Command = "SWITCH";
                                    pac.CommandArgs = collidingPac.GetBetterType();
                                }
                            }

                            pac.Command = "MOVE";
                            pac.CommandArgs = closest ?? map.OrderBy(_ => Guid.NewGuid()).First().Position;
                        }
                    }

                    Console.WriteLine($"{string.Join("|", pacs.Where(_ => _.IsMine).Select(_ => $"{_.GetFullCommand()}"))}");
                }
            }
        }

        public class Tile : AStarSearch.Location
        {
            public Tile(Position position, bool isWall) : base(position, isWall)
            {
            }
        }

        public enum PacType
        {
            ROCK,
            PAPER,
            SCISSORS
        }

        public class Pac
        {
            public Position Position { get; set; }
            public Position PreviousPosition { get; set; }

            public bool IsMine { get; set; }

            public string Command { get; set; }
            public object CommandArgs { get; set; }
            public int Id { get; set; }

            public PacType PacType { get; set; }
            public bool IsAlive { get; set; }

            public string GetFullCommand()
            {
                var result = $"{Command} {Id} {CommandArgs}";
                
                return result;
            }

            public PacType GetBetterType()
            {
                switch (PacType)
                {
                    case PacType.ROCK:
                        return PacType.PAPER;
                    case PacType.PAPER:
                        return PacType.SCISSORS;
                    case PacType.SCISSORS:
                        return PacType.ROCK;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class Pellet : Position
        {
            public Pellet(int x, int y) : base(x, y)
            {
            }

            public int Value { get; set; }
        }

        public class Position
        {
            public int X, Y;

            public string XY => X + " " + Y;
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int Manhattan(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

            public override string ToString()
            {
                return X + " " + Y;
            }
        }

        public class InputProcessor
        {
            public bool IsDebug { get; }
            private readonly Queue<string> _debugInputLines = new Queue<string>();
            private readonly List<string> _readLines = new List<string>();

            public InputProcessor(string debugData = null)
            {
                if (string.IsNullOrEmpty(debugData))
                    return;

                IsDebug = true;

                foreach (var s in debugData.Split('§'))
                {
                    _debugInputLines.Enqueue(s);
                }
            }

            public string ReadLine()
            {
                var line = IsDebug ? _debugInputLines.Dequeue() : Console.ReadLine();

                _readLines.Add(line);

                return line;
            }

            public void WriteDebugData()
            {
                Console.Error.WriteLine(string.Join("§", _readLines));
            }
        }

        public class AStarSearch
        {
            public List<Location> Map { get; }

            public AStarSearch(List<Location> map)
            {
                Map = map;
            }

            public Route Compute(Position start, Position end)
            {
                var openList = new List<Location>();
                var closedList = new List<Location>();
                var hasFoundPath = false;
                var movementCost = 0;

                // start by adding the original position to the open list
                var startLocation = Map.FirstOrDefault(_ => _.Position.XY == start.XY);
                if (startLocation != null)
                    openList.Add(startLocation);

                var endLocation = Map.FirstOrDefault(_ => _.Position.XY == end.XY);

                while (openList.Count > 0)
                {
                    // get the square with the lowest F score
                    var lowest = openList.Min(l => l.TotalScore);
                    var current = openList.First(l => l.TotalScore == lowest);

                    // add the current square to the closed list
                    closedList.Add(current);

                    // remove it from the open list
                    openList.Remove(current);

                    // if we added the destination to the closed list, we've found a path
                    if (current.Position.XY == end.XY)
                    {
                        hasFoundPath = true;
                        break;
                    }

                    var adjacentSquares = current.GetAdjacentSquares(Map).Where(_ => !_.IsWall).ToList();
                    movementCost++;

                    foreach (var adjacentSquare in adjacentSquares)
                    {
                        // if this adjacent square is already in the closed list, ignore it
                        if (closedList.FirstOrDefault(l => l.Position.X == adjacentSquare.Position.X
                                                           && l.Position.Y == adjacentSquare.Position.Y) != null)
                            continue;

                        // if it's not in the open list...
                        if (openList.FirstOrDefault(l => l.Position.X == adjacentSquare.Position.X
                                                         && l.Position.Y == adjacentSquare.Position.Y) == null)
                        {
                            // compute its score, set the parent
                            adjacentSquare.CostFromStart = movementCost;
                            adjacentSquare.CostToDestination = adjacentSquare.Position.Manhattan(end);
                            adjacentSquare.Parent = current;

                            // and add it to the open list
                            openList.Insert(0, adjacentSquare);
                        }
                        else
                        {
                            // test if using the current score makes the adjacent square's F score lower, if yes update the parent because it means it's a better path
                            if (movementCost + adjacentSquare.CostToDestination < adjacentSquare.TotalScore)
                            {
                                adjacentSquare.CostFromStart = movementCost;
                                adjacentSquare.Parent = current;
                            }
                        }
                    }
                }

                if (!hasFoundPath)
                    return new Route();

                var result = new List<Location>();
                var currentResult = endLocation;

                while (currentResult != null)
                {
                    result.Add(currentResult);
                    currentResult = currentResult.Parent;
                }

                result.Reverse();
                result.Remove(startLocation);

                var route = new Route
                {
                    Positions = result.Select(x => x.Position).ToList()
                };

                return route;
            }

            public class Location
            {
                public Location(Position position, bool isWall)
                {
                    Position = position;
                    IsWall = isWall;
                }

                public Position Position { get; }
                public bool IsWall { get; set; }

                /// <summary>
                /// Score
                /// </summary>
                public int TotalScore => CostFromStart + CostToDestination;
                /// <summary>
                /// Movement cost from start to here
                /// </summary>
                public int CostFromStart { get; set; }
                /// <summary>
                /// Estimated movement cost from here to destination
                /// </summary>
                public int CostToDestination { get; set; }

                public Location Parent { get; set; }

                public IEnumerable<Location> GetAdjacentSquares(List<Location> map)
                {
                    var proposedLocations = new List<Location>();

                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y - 1), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y + 1), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X - 1 && _.Position.Y == Position.Y), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X + 1 && _.Position.Y == Position.Y), proposedLocations);

                    return proposedLocations;
                }

                public static void AddIfNotNull<T>(T item, List<T> list)
                {
                    if (item != null)
                        list.Add(item);
                }
            }

            public class Route
            {
                public List<Position> Positions { get; set; } = new List<Position>();

                public bool IsValid => Positions.Any();

                public int Distance => Positions.Count;

                public override string ToString()
                {
                    return $"[Distance: {Distance}] {string.Join("->", Positions.Select(x => x.XY))}";
                }
            }
        }

    }
}