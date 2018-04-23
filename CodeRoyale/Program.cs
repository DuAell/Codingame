using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CodeRoyale
{
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();

            game.Launch();
        }
    }

    public class Game
    {
        public List<Unit> Units { get; set; }

        public List<Site> Sites { get; set; } = new List<Site>();

        public Gamer Me = new Gamer(true);

        public Gamer Ennemy = new Gamer(false);

        public string ConsoleInitDataDebug { get; set; } = string.Empty;
        public string ConsoleLoopDataDebug { get; set; } = string.Empty;

        public Position SpawnPoint { get; set; }

        public void Launch()
        {
            Init();
        }

        private StringReader _debugStringReader;
        public void LaunchDebug(string data)
        {
            using (_debugStringReader = new StringReader(data))
            {
                Init();
            }
        }

        private string ConsoleReadLine(bool isInit = false)
        {
            string data;
            if (_debugStringReader != null)
                data = _debugStringReader.ReadLine();
            else
            {
                data = Console.ReadLine();
                if (isInit)
                    ConsoleInitDataDebug += $@"{data}\n";
                else
                    ConsoleLoopDataDebug += $@"{data}\n";
            }
            return data;
        }

        private void Init()
        {
            string[] inputs;
            var numSites = int.Parse(ConsoleReadLine(true));
            for (var i = 0; i < numSites; i++)
            {
                inputs = ConsoleReadLine(true).Split(' ');
                Sites.Add(new Site(inputs));
            }

            // game loop
            while (true)
            {
                Units = new List<Unit>();

                var data = ConsoleReadLine();
                if (data == null) break;
                var inputsGamer = data.Split(' ');
                
                for (var i = 0; i < numSites; i++)
                {
                    inputs = ConsoleReadLine().Split(' ');
                    var siteId = int.Parse(inputs[0]);
                    var site = Sites.Single(x => x.SiteId == siteId);
                    Sites.Remove(site);
                    Sites.Add(site.SwitchToCorrectType(inputs));
                    Sites.Single(x => x.SiteId == siteId).Update(inputs);
                }

                var numUnits = int.Parse(ConsoleReadLine());
                for (var i = 0; i < numUnits; i++)
                {
                    Units.Add(Unit.Create(ConsoleReadLine().Split(' ')));
                }

                Me.Update(inputsGamer, this);
                Ennemy.Update(null, this);

                Console.Error.WriteLine(ConsoleInitDataDebug + ConsoleLoopDataDebug);

                if (SpawnPoint == null)
                    SpawnPoint = Me.Queen;

                GameLogic();

                ConsoleLoopDataDebug = string.Empty;
            }
        }

        private void GameLogic()
        {
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // First line: A valid queen action
            // Second line: A set of training instructions
            QueenLogic();
            TrainingLogic();
        }

        private void QueenLogic()
        {
            DecideWhatToBuild();
            //if (Me.TouchedSite != null && Me.TouchedSite.Owner == Owner.None)
            //    DecideWhatToBuild();
            //else if (Sites.Any(x => x.Owner == Owner.None))
            //    Console.WriteLine($"MOVE {Sites.Where(x => x.Owner == Owner.None).OrderBy(x => x.Distance(Me.Queen)).First().XY}");
            //else
            //    Console.WriteLine("WAIT");
        }

        private void TrainingLogic()
        {
            var trainingSites = new List<Barracks>();
            Barracks trainingSite;
            do
            {
                trainingSite = DecideWhatToTrain(trainingSites);
                if (trainingSite == null) continue;

                Me.Gold -= trainingSite.GetCost();
                trainingSites.Add(trainingSite);
            } while (trainingSite != null);

            if (!trainingSites.Any())
                Console.WriteLine("TRAIN");
            else
                Console.WriteLine($"TRAIN {string.Join(" ", trainingSites.Select(x => x.SiteId))}");
        }

        private void DecideWhatToBuild()
        {
            // Construire une barracks
            // Construire des mines
            // Construire une tour
            // Dès qu'il y a des troupes ennemies, réparer la tour

            if (Me.TouchedSite is Mine mine && mine.CanBeUpgraded())
            {
                Console.WriteLine($"BUILD {Me.TouchedSite.SiteId} MINE");
                return;
            }

            var closestFreeSite = Sites.OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault(x => x.Owner == Owner.None);

            if (Me.Sites.OfType<Mine>().Count() < 3 && closestFreeSite != null)
            {
                Console.WriteLine($"BUILD {closestFreeSite.SiteId} MINE");
                return;
            }

            if (Me.Sites.OfType<Barracks>().All(x => x.ProductionType != ProductionType.Knight))
            {
                if (closestFreeSite != null)
                {
                    Console.WriteLine($"BUILD {closestFreeSite.SiteId} BARRACKS-KNIGHT");
                    return;
                }
            }

            // Si la reine est à portée d'une tour ennemie, on se casse !
            if (Ennemy.Sites.OfType<Tower>().Any(x => x.AttackRange >= x.Distance(Me.Queen)))
            {
                Console.WriteLine($"MOVE {SpawnPoint.XY}");
                return;
            }

            //if (Me.Sites.OfType<Barracks>().All(x => x.ProductionType != ProductionType.Archer))
            //{
            //    Console.WriteLine($"BUILD {Me.TouchedSite.SiteId} BARRACKS-ARCHER");
            //    return;
            //}

            //if (Me.Sites.OfType<Barracks>().All(x => x.ProductionType != ProductionType.Giant))
            //{
            //    Console.WriteLine($"BUILD {Me.TouchedSite.SiteId} BARRACKS-GIANT");
            //    return;
            //}

            // Réparer les tours

            // Attaque de la reine sur les casernes adverses

            if (Ennemy.Sites.OfType<Barracks>().Any())
            {
                var target = Ennemy.Sites.OfType<Barracks>().First();

                var closestFreeSiteInDirectionOfBarracks = Sites.Where(x => x.Owner == Owner.None && x.Distance(Me.Queen) < target.Distance(Me.Queen) && !x.IsAtAttackRange(Ennemy.Towers)).OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault();

                if (closestFreeSiteInDirectionOfBarracks != null)
                    Console.WriteLine($"BUILD {closestFreeSiteInDirectionOfBarracks.SiteId} TOWER");
                else
                    Console.WriteLine($"MOVE {Ennemy.Sites.OfType<Barracks>().First().XY}");
                return;
            }

            if (Ennemy.Units.OfType<Knight>().Any(x => x.Distance(Me.Queen) < 800) && closestFreeSite != null && closestFreeSite.Distance(Me.Queen) < 500)
            {
                Console.WriteLine($"BUILD {closestFreeSite.SiteId} TOWER");
                return;
            }

            if (Ennemy.Units.OfType<Knight>().Any(x => x.Distance(Me.Queen) < 2000))
            {
                var closestTowerSite = Sites.Where(x => 
                        x.Owner == Owner.None || 
                        (x is Tower tower && tower.Owner == Owner.Me && tower.CanBeUpgraded()))
                    .OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault();
                if (closestTowerSite != null)
                {
                    Console.WriteLine($"BUILD {closestTowerSite.SiteId} TOWER"); 
                    return;
                }
            }

            if (closestFreeSite != null)
            {
                Console.WriteLine($"BUILD {closestFreeSite.SiteId} MINE");
                return;
            }

            //if (Sites.All(x => x.Owner != Owner.None))
            //{
            //    var closestTower = Me.Sites.OfType<Tower>().OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault();
            //    if (closestTower != null)
            //    {
            //        Console.WriteLine($"BUILD {closestTower.SiteId} TOWER");
            //        return;
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"BUILD {Me.TouchedSite.SiteId} TOWER");
            //    return;
            //}

            Console.WriteLine("WAIT");
        }

        private Barracks DecideWhatToTrain(List<Barracks> trainingSites)
        {
            //var needToProduceArcher = Units.Count(x => x is Knight && !x.IsAllied) > Units.Count(x => x is Archer && x.IsAllied) * 2;
            var sitesReady = Me.Sites.OfType<Barracks>().Where(x => !trainingSites.Contains(x) && x.TurnsLeftBeforeBuilding == 0).ToList();
            if (!sitesReady.Any()) return null;

            if (sitesReady.Any(x => x.ProductionType == ProductionType.Archer) && Me.Gold > Archer.Cost && !Me.Units.OfType<Archer>().Any())
                return sitesReady.Where(x => x.ProductionType == ProductionType.Archer).OrderBy(x => x.Distance(Me.Queen)).First();

            if (sitesReady.Any(x => x.ProductionType == ProductionType.Knight) && Me.Gold > Knight.Cost)
                return sitesReady.Where(x => x.ProductionType == ProductionType.Knight).OrderBy(x => x.Distance(Ennemy.Queen)).First();

            if (Ennemy.Sites.OfType<Tower>().Any() && sitesReady.Any(x => x.ProductionType == ProductionType.Giant) && Me.Gold > Giant.Cost && !Me.Units.OfType<Giant>().Any())
                return sitesReady.Where(x => x.ProductionType == ProductionType.Giant).OrderBy(x => x.Distance(Ennemy.Sites.OfType<Tower>().First())).First();

            return null;
        }
    }

    public class Gamer
    {
        private readonly bool _isMe;
        public int Gold { get; set; }
        public Site TouchedSite { get; private set; }
        public Queen Queen { get; set; }
        public List<Site> Sites { get; private set; }
        public List<Unit> Units { get; private set; }
        public List<Tower> Towers => Sites.OfType<Tower>().ToList();

        public Gamer(bool isMe)
        {
            _isMe = isMe;
        }

        public void Update(string[] inputs, Game game)
        {
            if (inputs != null)
            {
                Gold = int.Parse(inputs[0]);
                TouchedSite = game.Sites.SingleOrDefault(x => x.SiteId == int.Parse(inputs[1])); // -1 if none
            }

            Queen = game.Units.OfType<Queen>().Single(x => x.IsAllied == _isMe);
            Sites = game.Sites.Where(x => x.Owner == (_isMe ? Owner.Me : Owner.Ennemy)).ToList();
            Units = game.Units.Where(x => x.IsAllied == _isMe).ToList();
        }
    }

    public class Position
    {
        public int X;
        public int Y;
        public string XY => $"{X} {Y}";

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        private static double Sqr(double a)
        {
            return a * a;
        }

        public int Distance(Position position)
        {
            return (int) Math.Sqrt(Sqr(position.Y - Y) + Sqr(position.X - X));
        }
    }

    public enum Owner
    {
        None,
        Me,
        Ennemy
    }

    public enum ProductionType
    {
        Knight,
        Archer,
        Giant
    }

    public class Site : Position
    {
        public int SiteId { get; }

        public int Radius { get; }

        public Owner Owner { get; private set; }

        public Site(string[] inputs) : base(int.Parse(inputs[1]), int.Parse(inputs[2]))
        {
            SiteId = int.Parse(inputs[0]);
            Radius = int.Parse(inputs[3]);
        }

        public Site(Site site) : base(site.X, site.Y)
        {
            SiteId = site.SiteId;
            Radius = site.Radius;
        }

        public Site SwitchToCorrectType(string[] inputs)
        {
            switch (int.Parse(inputs[3]))
            {
                case -1:
                    if (GetType() == typeof(Site)) return this;
                    return new Site(this);
                case 0:
                    if (GetType() == typeof(Mine)) return this;
                    return new Mine(this);
                case 1:
                    if (GetType() == typeof(Tower)) return this;
                    return new Tower(this);
                case 2:
                    if (GetType() == typeof(Barracks)) return this;
                    return new Barracks(this);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void Update(string[] inputs)
        {
            //int ignore1 = int.Parse(inputs[1]); // used in future leagues
            //int ignore2 = int.Parse(inputs[2]); // used in future leagues

            switch (int.Parse(inputs[4]))
            {
                case -1:
                    Owner = Owner.None;
                    break;
                case 0:
                    Owner = Owner.Me;
                    break;
                case 1:
                    Owner = Owner.Ennemy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsAtAttackRange(IEnumerable<Tower> ennemyTowers)
        {
            return ennemyTowers.Any(x => x.AttackRange > x.Distance(this));
        }
    }

    public abstract class Unit : Position
    {
        protected Unit(string[] inputs) : base(int.Parse(inputs[0]), int.Parse(inputs[1]))
        {
            IsAllied = int.Parse(inputs[2]) == 0;
            Health = int.Parse(inputs[4]);
        }

        public bool IsAllied { get; }

        public int Health { get; }

        public static Unit Create(string[] inputs)
        {
            switch (int.Parse(inputs[3]))
            {
                case -1:
                    return new Queen(inputs);
                case 0:
                    return new Knight(inputs);
                case 1:
                    return new Archer(inputs);
                case 2:
                    return new Giant(inputs);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Queen : Unit
    {
        public Queen(string[] inputs) : base(inputs)
        {
        }
    }

    public class Knight : Unit
    {
        public Knight(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 80;
    }

    public class Archer : Unit
    {
        public Archer(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 100;
    }

    public class Giant : Unit
    {
        public Giant(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 140;
    }

    public class Barracks : Site
    {
        public int TurnsLeftBeforeBuilding { get; private set; }

        public ProductionType ProductionType { get; private set; }

        public Barracks(string[] inputs) : base(inputs)
        {
        }

        public Barracks(Site site) : base(site)
        {
        }

        public override void Update(string[] inputs)
        {
            base.Update(inputs);

            TurnsLeftBeforeBuilding = int.Parse(inputs[5]);

            switch (int.Parse(inputs[6]))
            {
                case 0:
                    ProductionType = ProductionType.Knight;
                    break;
                case 1:
                    ProductionType = ProductionType.Archer;
                    break;
                case 2:
                    ProductionType = ProductionType.Giant;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetCost()
        {
            switch (ProductionType)
            {
                case ProductionType.Knight:
                    return Knight.Cost;
                case ProductionType.Archer:
                    return Archer.Cost;
                case ProductionType.Giant:
                    return Giant.Cost;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Tower : Site
    {
        public int Health { get; private set; }

        public int AttackRange { get; private set; }

        public Tower(string[] inputs) : base(inputs)
        {
        }

        public Tower(Site site) : base(site)
        {
        }

        public override void Update(string[] inputs)
        {
            base.Update(inputs);

            Health = int.Parse(inputs[5]);

            AttackRange = int.Parse(inputs[6]);
        }

        public bool CanBeUpgraded()
        {
            return AttackRange <= 510;
        }
    }

    public class Mine : Site
    {
        public int ProductionRate { get; private set; }
        public int RemainingGold { get; private set; }
        public int MaxProductionRate { get; private set; }

        public Mine(string[] inputs) : base(inputs)
        {
        }

        public Mine(Site site) : base(site)
        {
        }

        public override void Update(string[] inputs)
        {
            base.Update(inputs);

            RemainingGold = int.Parse(inputs[1]);
            MaxProductionRate = int.Parse(inputs[2]);
            ProductionRate = int.Parse(inputs[5]);
        }

        public bool CanBeUpgraded()
        {
            return ProductionRate < MaxProductionRate && RemainingGold >= 100;
        }
    }
}