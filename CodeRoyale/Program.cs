using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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
        public static List<Unit> Units { get; set; }

        public static List<Site> Sites { get; set; } = new List<Site>();

        public Gamer Me = new Gamer(Owner.Me);

        public Gamer Ennemy = new Gamer(Owner.Ennemy);

        public string ConsoleInitDataDebug { get; set; } = string.Empty;
        public string ConsoleLoopDataDebug { get; set; } = string.Empty;

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
                Sites.Add(new FreeSite(inputs));
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

                GameLogic();

                ConsoleLoopDataDebug = string.Empty;
            }
        }

        private void GameLogic()
        {
            // First line: A valid queen action
            // Second line: A set of training instructions
            QueenLogic();
            TrainingLogic();
        }

        private void QueenLogic()
        {
            var result = DecideWhatToBuild();
            Console.WriteLine(result);
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

        private string DecideWhatToBuild()
        {
            // Construire une barracks
            // Construire des mines
            // Construire une tour
            // Dès qu'il y a des troupes ennemies, réparer la tour
            var backupTower = Me.Towers.OrderBy(x => x.Distance(Me.SpawnPoint)).FirstOrDefault();

            if (Me.Queen.IsAtEnnemyRange(Ennemy) && backupTower != null && backupTower.CanBeUpgraded())
            {
                if (Me.TouchedSite is FreeSite)
                    return $"BUILD {Me.TouchedSite.SiteId} TOWER";
                if (!(Me.TouchedSite == backupTower && !backupTower.CanBeUpgraded()))
                    return $"BUILD {backupTower.SiteId} TOWER";
            }

            if (Me.TouchedSite is Mine mine && mine.Owner == Owner.Me && mine.CanBeUpgraded())
            {
                return $"BUILD {Me.TouchedSite.SiteId} MINE";
            }

            var closestFreeSiteForMine =
                Sites.Where(x =>
                    x.Owner == Owner.None && x.Distance(Ennemy.SpawnPoint) > 1000 && (x.RemainingGold == null || x.RemainingGold > 20) &&
                    !x.IsAtEnnemyRange(Ennemy)).OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault();

            var closestFreeSite = Sites.OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault(x => x.Owner == Owner.None);

            if (Me.Sites.OfType<Barracks>().All(x => x.ProductionType != ProductionType.Knight) && !Me.Queen.IsAtEnnemyRange(Ennemy))
            {
                if (closestFreeSite != null)
                {
                    return $"BUILD {closestFreeSite.SiteId} BARRACKS-KNIGHT";
                }
            }

            if (!Ennemy.Units.OfType<Knight>().Any() && closestFreeSiteForMine != null)
            {
                return $"BUILD {closestFreeSiteForMine.SiteId} MINE";
            }

            if (!Me.Towers.Any() && closestFreeSite != null)
                return $"BUILD {closestFreeSite.SiteId} TOWER";

            var closestFreeSiteForBarracks = Sites.Where(x => x.Owner == Owner.None && x.RemainingGold == 0 && !x.IsAtEnnemyRange(Ennemy))
                .OrderBy(x => x.Distance(Me.Queen)).FirstOrDefault();
            if (closestFreeSiteForBarracks != null)
            {
                return $"BUILD {closestFreeSiteForBarracks.SiteId} BARRACKS-KNIGHT";
            }

            // Si la reine est à portée d'une tour ennemie, on se casse !
            if (Ennemy.Sites.OfType<Tower>().Any(x => x.AttackRange >= x.Distance(Me.Queen)))
            {
                return $"MOVE {Me.SpawnPoint.XY}";
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

            // Attaque de la reine sur les casernes adverses sans défense
            var ennemyBarracksWithoutDefense =
                Ennemy.Sites.OfType<Barracks>().FirstOrDefault(x => !x.IsAtEnnemyRange(Ennemy));
            if (ennemyBarracksWithoutDefense != null) 
            {
                return $"MOVE {ennemyBarracksWithoutDefense.XY}";
            }

            //// Build tower urgently
            //if (Ennemy.Units.OfType<Knight>().Any(x => x.Distance(Me.Queen) < 800) && closestFreeSite != null && closestFreeSite.Distance(Me.Queen) < 500)
            //{
            //    return $"BUILD {closestFreeSite.SiteId} TOWER";
            //}

            // Repair tower
            var towerToRepair = Me.Towers.Where(x => x.NeedRepair()).OrderBy(x => x.Health).FirstOrDefault();
            if (towerToRepair != null)
                return $"BUILD {towerToRepair.SiteId} TOWER";

            if (Me.TouchedSite is Tower tower && tower.CanBeUpgraded())
                return $"BUILD {Me.TouchedSite.SiteId} TOWER";

            var closestFreeSiteToEnnemySpawnPoint = Sites.Where(x => x.Owner == Owner.None && !x.IsAtEnnemyRange(Ennemy))
                .OrderBy(x => x.Distance(Ennemy.SpawnPoint)).FirstOrDefault();
            if (closestFreeSiteToEnnemySpawnPoint != null)
            {
                return $"BUILD {closestFreeSiteToEnnemySpawnPoint.SiteId} TOWER";
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

            // Don't know what to do, back to tower.
            if (backupTower != null)
                return $"BUILD {backupTower.SiteId} TOWER";

            return "WAIT";
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
        private readonly Owner _owner;
        public int Gold { get; set; }
        public Site TouchedSite { get; private set; }
        public Queen Queen { get; set; }
        public List<Site> Sites { get; private set; }
        public List<Unit> Units { get; private set; }
        public List<Tower> Towers => Sites.OfType<Tower>().ToList();

        public Position SpawnPoint { get; set; }

        public Gamer(Owner owner)
        {
            _owner = owner;
        }

        public void Update(string[] inputs, Game game)
        {
            if (inputs != null)
            {
                Gold = int.Parse(inputs[0]);
                TouchedSite = Game.Sites.SingleOrDefault(x => x.SiteId == int.Parse(inputs[1])); // -1 if none
            }

            Queen = Game.Units.OfType<Queen>().Single(x => x.Owner == _owner);
            Sites = Game.Sites.Where(x => x.Owner == _owner).ToList();
            Units = Game.Units.Where(x => x.Owner == _owner).ToList();

            if (SpawnPoint == null)
                SpawnPoint = Queen;
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

    public abstract class BaseUnit : Position
    {
        public Owner Owner { get; protected set; }

        public abstract int Radius { get; protected set; }

        public abstract int AttackRange { get; set; }

        public abstract int Speed { get; }

        protected BaseUnit(int x, int y) : base(x, y)
        {
        }

        public abstract bool CanAttack(BaseUnit unit);

        public bool IsAtAttackRange(BaseUnit unit)
        {
            return Distance(unit) < AttackRange;
        }

        public bool IsAtEnnemyRange(Gamer ennemy)
        {
            // Contact = distance - rayon1 - rayon2 < 5

            return ennemy.Towers.Any(x => x.AttackRange > (x.Distance(this) - Radius - x.Radius)) ||
                   ennemy.Units.Where(x => x.CanAttack(this)).Any(x => x.AttackRange + x.Speed > (x.Distance(this) - Radius - x.Radius));
        }
    }

    public abstract class Site : BaseUnit
    {
        public int SiteId { get; }
        public int? RemainingGold { get; private set; }
        public int? MaxProductionRate { get; private set; }

        protected Site(string[] inputs) : base(int.Parse(inputs[1]), int.Parse(inputs[2]))
        {
            SiteId = int.Parse(inputs[0]);
            Radius = int.Parse(inputs[3]);
        }

        protected Site(Site site) : base(site.X, site.Y)
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
                    return new FreeSite(this);
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

            var remainingGold = int.Parse(inputs[1]);
            if (remainingGold != -1)
                RemainingGold = remainingGold;

            var maxProductionRate = int.Parse(inputs[2]);
            if (maxProductionRate != -1)
                MaxProductionRate = maxProductionRate;

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

        public abstract override bool CanAttack(BaseUnit unit);

        public override int Speed { get; } = 0;
    }

    public abstract class Unit : BaseUnit
    {
        protected Unit(string[] inputs) : base(int.Parse(inputs[0]), int.Parse(inputs[1]))
        {
            switch (int.Parse(inputs[2]))
            {
                case 0:
                    Owner = Owner.Me;
                    break;
                case 1:
                    Owner = Owner.Ennemy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Health = int.Parse(inputs[4]);
        }

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

        public override int Speed { get; } = 60;
        public override int Radius { get; protected set; } = 30;
        public override int AttackRange { get; set; } = 0;

        public override bool CanAttack(BaseUnit unit)
        {
            if (unit.Owner == Owner) return false;
            if (unit is Mine) return true;
            if (unit is Barracks) return true;
            return false;
        }
    }

    public class Knight : Unit
    {
        public Knight(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 80;
        public override int Speed { get; } = 100;
        public override int Radius { get; protected set; } = 20;
        public override int AttackRange { get; set; } = 0;
        public override bool CanAttack(BaseUnit unit)
        {
            if (unit.Owner == Owner) return false;
            if (unit is Queen) return true;
            if (unit is Mine) return true;
            return false;
        }
    }

    public class Archer : Unit
    {
        public Archer(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 100;
        public override int Speed { get; } = 75;
        public override int Radius { get; protected set; } = 25;
        public override int AttackRange { get; set; } = 200;
        public override bool CanAttack(BaseUnit unit)
        {
            if (unit.Owner == Owner) return false;
            if (unit is Unit) return true;
            if (unit is Mine) return true;
            return false;
        }
    }

    public class Giant : Unit
    {
        public Giant(string[] inputs) : base(inputs)
        {
        }

        public static int Cost { get; } = 140;
        public override int Speed { get; } = 50;
        public override int Radius { get; protected set; } = 40;
        public override int AttackRange { get; set; } = 0;

        public override bool CanAttack(BaseUnit unit)
        {
            if (unit.Owner == Owner) return false;
            if (unit is Tower) return true;
            if (unit is Mine) return true;
            return false;
        }
    }

    public class FreeSite : Site
    {
        
        public FreeSite(string[] inputs) : base(inputs)
        {
        }

        public FreeSite(Site site) : base(site)
        {
        }

        public override int Radius { get; protected set; } = 50; // Au pif
        public override int AttackRange { get; set; } = 0;

        public override bool CanAttack(BaseUnit unit)
        {
            return false;
        }
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

        public override int Radius { get; protected set; } = 50; // Au pif
        public override int AttackRange { get; set; } = 0;

        public override bool CanAttack(BaseUnit unit)
        {
            return false;
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

        public override int Radius { get; protected set; } = 80; // Au pif
        public override int AttackRange { get; set; }

        public override bool CanAttack(BaseUnit unit)
        {
            if (unit.Owner == Owner) return false;

            if (unit is Queen)
            {
                var ennemyUnits = Game.Units.Where(x => x.Owner == unit.Owner);
                return ennemyUnits.Any(x => x.IsAtAttackRange(this) && !(x is Queen));
            }

            if (unit is Knight || unit is Archer || unit is Giant) return true;

            return false;

        }

        public bool CanBeUpgraded()
        {
            return Health < 750;
        }

        public bool NeedRepair()
        {
            return Health < 50;
        }
    }

    public class Mine : Site
    {
        public int ProductionRate { get; private set; }
        
        public Mine(string[] inputs) : base(inputs)
        {
        }

        public Mine(Site site) : base(site)
        {
        }

        public override void Update(string[] inputs)
        {
            base.Update(inputs);
            ProductionRate = int.Parse(inputs[5]);
        }

        public override int Radius { get; protected set; } = 50; // Au pif
        public override int AttackRange { get; set; }

        public override bool CanAttack(BaseUnit unit)
        {
            return false;
        }

        public bool CanBeUpgraded()
        {
            if (MaxProductionRate == null) return true;
            return ProductionRate < MaxProductionRate && RemainingGold >= 100;
        }
    }
}