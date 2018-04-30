using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BottersOfTheGalaxy
{
/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
    public class Player
    {
        private static string _debugInitInput;
        private static string _debugInput;
        private static int _myTeam;
        public static readonly List<Bush> Bushes = new List<Bush>();
        public static readonly List<Item> Items = new List<Item>();
        public static List<Unit> Units;
        private static int _roundType; // a positive value will show the number of heroes that await a command
        private static StringReader _stringReader;
        private static List<Team> _teams;

        public static Team Me;
        public static Team Ennemy;
        public static readonly Team Neutral = new Team(-1);

        private static string ReadLine(bool isInit = false)
        {
            if (_stringReader != null) return _stringReader.ReadLine();

            var input = Console.ReadLine();
            if (isInit)
                _debugInitInput += input + @"\n";
            else
                _debugInput += input + @"\n";
            return input;
        }

        public void DebugMain(string data)
        {
            using (var sr = new StringReader(data))
            {
                _stringReader = sr;
                LaunchGame();
            }
        }

        public static void Main(string[] args)
        {
            new Player().LaunchGame();
        }

        private static Unit ReadUnits(string[] inputs)
        {
            Unit unit;
            switch (inputs[2])
            {
                case "UNIT":
                    unit = new Creep(inputs);
                    break;
                case "HERO":
                    unit = Hero.GetHero(inputs);
                    break;
                case "TOWER":
                    unit = new Tower(inputs);
                    break;
                case "GROOT":
                    unit = new Groot(inputs);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Units.Add(unit);
            return unit;
        }

        public static IEnumerable<UnitWithValue> GetUnitsWithValues(Hero hero)
        {
            return Units.Select(x => new UnitWithValue(x, x.GetValueForHero(hero)))
                .Where(x => x.Value > 0).OrderByDescending(x => x.Value);
        }

        private void LaunchGame()
        {
            string[] inputs;
            _myTeam = int.Parse(ReadLine(true));

            var bushAndSpawnPointCount =
                int.Parse(ReadLine(
                    true)); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
            for (var i = 0; i < bushAndSpawnPointCount; i++)
            {
                inputs = ReadLine(true).Split(' ');
                Bushes.Add(new Bush(inputs));
            }

            var itemCount = int.Parse(ReadLine(true)); // useful from wood2
            for (var i = 0; i < itemCount; i++)
            {
                inputs = ReadLine(true).Split(' ');
                Items.Add(new Item(inputs));
            }

            Me = new Team(_myTeam);
            Ennemy = new Team(_myTeam == 0 ? 1 : 0);
            _teams = new List<Team>
            {
                Me,
                Ennemy,
                Neutral
            };

            // game loop
            do
            {
                _debugInput = string.Empty;

                Me.Gold = int.Parse(ReadLine());
                Ennemy.Gold = int.Parse(ReadLine());
                _roundType =
                    int.Parse(ReadLine()); // a positive value will show the number of heroes that await a command

                var previousUnits = new List<Unit>();
                if (Units != null)
                    previousUnits.AddRange(Units);
                Units = new List<Unit>();
                var entityCount = int.Parse(ReadLine());
                for (var i = 0; i < entityCount; i++)
                {
                    inputs = ReadLine().Split(' ');
                    var unit = ReadUnits(inputs);
                    unit.SetPreviousTurn(previousUnits.SingleOrDefault(x => x.UnitId == unit.UnitId));

                    var team = _teams.Single(x => x.TeamId == unit.TeamId);
                    unit.Team = team;
                }

                _teams.ForEach(x => x.Units = Units.Where(u => u.Team == x).ToList());

                Console.Error.WriteLine(_debugInitInput + _debugInput);

                GameLogic();
            } while (_stringReader == null);
        }

        private void GameLogic()
        {
            // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
            // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"

            CreateMap();

            if (_roundType == -2)
            {
                Console.WriteLine("IRONMAN");
                return;
            }

            if (_roundType == -1)
            {
                Console.WriteLine("VALKYRIE");
                return;
            }

            UpdateMap();

            foreach (var myHero in Me.Units.OfType<Hero>())
            {
                var result = myHero.Logic();
                Console.Error.WriteLine($"{myHero.HeroType}: {result}");
                Console.WriteLine(result);
            }
        }

        public static Dictionary<Position, int> Map;

        private static void CreateMap()
        {
            if (Map != null) return;

            Map = new Dictionary<Position, int>(1921*751);
            for (var x = 0; x <= 1920; x = x + 20)
            {
                for (var y = 0; y <= 750; y = y + 20)
                {
                    Map.Add(new Position(x, y), 0);
                }
            }
        }

        private static void UpdateMap()
        {
            foreach (var positionValue in Map.Where(x => x.Value != 0).ToList())
            {
                Map[positionValue.Key] = 0;
            }

            foreach (var unit in Ennemy.Units)
            {
                Map.Where(x => unit.IsAtMoveAttackDistance(x.Key, x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] + 1);
            }
            foreach (var unit in Ennemy.Units.OfType<Creep>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] + 2);
            }
            foreach (var unit in Ennemy.Units.OfType<Hero>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] + 5);
            }
            foreach (var unit in Ennemy.Units.OfType<Tower>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] + 10);
            }

            foreach (var unit in Me.Units.OfType<Creep>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] - 1);
            }
            foreach (var unit in Me.Units.OfType<Hero>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] - 3);
            }
            foreach (var unit in Me.Units.OfType<Tower>())
            {
                Map.Where(x => unit.IsAtAttackDistance(x.Key)).ToList().ForEach(x => Map[x.Key] = Map[x.Key] - 5);
            }

            //foreach (var positionValue in Map.OrderBy(x => x.Value))
            //{
            //    Console.Error.WriteLine($"Position {positionValue.Key.XY} : {positionValue.Value}");
            //}
        }
    }

    public class PositionValue : Position
    {
        public PositionValue(Position position, int value) : base(position.X, position.Y)
        {
            Value = value;
        }

        public PositionValue(int x, int y, int value) : base(x, y)
        {
            Value = value;
        }

        public int Value { get; set; }
    }

    public class UnitWithValue
    {
        public Unit Unit { get; }
        public double Value { get; }

        public UnitWithValue(Unit unit, double value)
        {
            Unit = unit;
            Value = value;
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
            return (int)Math.Sqrt(Sqr(position.Y - Y) + Sqr(position.X - X));
        }

        public Position Behind()
        {
            var ennemyTower = Player.Ennemy.Units.OfType<Tower>().Single();
            var alliedTower = Player.Me.Units.OfType<Tower>().Single();
            var moduloX = (ennemyTower.X > alliedTower.X) ? -1 : +1;
            return new Position(X + moduloX, Y);
        }

        //public double Distance(Position p2)
        //{
        //    return Math.Sqrt(Math.Pow(p2.X - X, 2) + Math.Pow(p2.Y - Y, 2));
        //}
    }

    public abstract class Unit : Position
    {
        public int UnitId;
        public string UnitType;
        public int TeamId;
        public int AttackRange;
        public int Health;
        public int MaxHealth;
        public int Shield;
        public int AttackDamage;
        public int MovementSpeed;
        public int StunDuration;
        public int GoldValue;

        public double AttackTime;
        private Unit _previousTurn;

        public double PercentLife => (double)Health / MaxHealth;

        public virtual void SetPreviousTurn(Unit previousTurn)
        {
            _previousTurn = previousTurn;
        }

        protected Unit(string[] inputs) : base(int.Parse(inputs[3]), int.Parse(inputs[4]))
        {
            UnitId = int.Parse(inputs[0]);
            UnitType = inputs[2];
            TeamId = int.Parse(inputs[1]);
            AttackRange = int.Parse(inputs[5]);
            Health = int.Parse(inputs[6]);
            MaxHealth = int.Parse(inputs[7]);
            Shield = int.Parse(inputs[8]); // useful in bronze
            AttackDamage = int.Parse(inputs[9]);
            MovementSpeed = int.Parse(inputs[10]);
            StunDuration = int.Parse(inputs[11]); // useful in bronze
            GoldValue = int.Parse(inputs[12]);
            AttackTime = 0.2;
        }

        public Team Team;

        public bool IsAtAttackDistance(Position target)
        {
            return IsAtAttackRangeFromPosition(this, target);
        }

        public bool IsAtAttackRangeFromPosition(Position position, Position target)
        {
            return AttackRange >= position.Distance(target);
        }

        public bool HasTimeToMoveAndAttack(Position position)
        {
            int moveTime = 0;
            if (MovementSpeed > 0)
                moveTime = Distance(position) / MovementSpeed;
            return moveTime + AttackTime <= 1.0;
        }

        public bool IsAtMoveAttackDistance(Position position, Position target)
        {
            return HasTimeToMoveAndAttack(position) && IsAtAttackRangeFromPosition(position, target);
        }

        public bool HasBeenHitLastTurn => _previousTurn != null && _previousTurn.Health > Health;

        public double GetValueForHero(Hero hero)
        {
            if (this is Hero && Team == hero.Team) return 0; // Do not kill my heroes
            if (this is Tower && Team == hero.Team) return 0; // Do not kill my tower !
            double value = 0;
            value += 10 * ((double)hero.AttackDamage / Health ); // Gives more value to wounded units
            if (PercentLife > 0.4 && Team == hero.Team) return 0; // Can't kill ally units with more than 40% life
            if (this is Creep) value += 1;
            if (this is Hero) value += 3;
            if (this is Tower) value += 5;
            if (Health / hero.AttackDamage < 1) value += 10; // Always try to kill units that can die this turn
            if (Player.Ennemy.Units.OfType<Tower>().Single().IsAtAttackDistance(hero) && this is Hero) return 0; // Do not attack ennemy heroes under tower


            if (Team == Player.Ennemy) value += 1; // Prefer ennemy

            return value;
        }

        public double GetAttackTime(Unit unit)
        {
            var dist = Distance(unit);
            double t = 0;
            if (dist > unit.AttackRange)
            {
                t = (dist - AttackRange) / MovementSpeed;
                dist = AttackRange;
            }

            t += AttackTime;
            if (AttackRange > 150)
            {
                t += AttackTime * (dist / AttackRange);
            }

            return t;
        }
    }

    public class Creep : Unit
    {
        public Creep(string[] inputs) : base(inputs)
        {
        }
    }

    public abstract class Hero : Unit
    {
        public int CountDown1;
        public int CountDown2;
        public int CountDown3;
        public int Mana;
        public int MaxMana;
        public int ManaRegeneration;
        public string HeroType;
        public bool IsVisible;
        public int ItemsOwned;

        public List<Item> Items = new List<Item>();

        // Contains() is too much time-consuming, use it after another filter
        public virtual IEnumerable<Item> GetItemsOfInterest()
        {
            return Player.Items.Where(x => x.Damage > 5).OrderByDescending(x => x.Damage);
        }

        public override void SetPreviousTurn(Unit previousTurn)
        {
            base.SetPreviousTurn(previousTurn);

            if (previousTurn == null) return;
            Items = ((Hero) previousTurn).Items;
        }

        protected Hero(string[] inputs) : base(inputs)
        {
            CountDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
            CountDown2 = int.Parse(inputs[14]);
            CountDown3 = int.Parse(inputs[15]);
            Mana = int.Parse(inputs[16]);
            MaxMana = int.Parse(inputs[17]);
            ManaRegeneration = int.Parse(inputs[18]);
            HeroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
            IsVisible = int.Parse(inputs[20]) == 1; // 0 if it isn't
            ItemsOwned = int.Parse(inputs[21]); // useful from wood1
            AttackTime = 0.1;
        }

        private string SellStuffBeforeDying()
        {
            if (PercentLife < 0.2)
            {
                var item = Items.OrderByDescending(x => x.ItemCost).FirstOrDefault();
                if (item != null)
                    return Sell(item);
            }

            return null;
        }

        public string BuyHealthPotion()
        {
            var potions = Player.Items
                .Where(x => x.IsPotion && x.Health > 0 && x.Health <= (MaxHealth - Health))
                .ToList();
                
            var affordableItem = potions.OrderByDescending(x => x.Health).FirstOrDefault(x => x.ItemCost <= Team.Gold);
            if (affordableItem != null && PercentLife < 0.5)
            {
                return Buy(affordableItem);
            }

            // We may have to sell stuff to buy it
            if (Player.Ennemy.Units.Any(x => x.IsAtAttackDistance(this))) return null; // Do not buy if ennemy too close
            if (Player.Neutral.Units.Any(x => x.IsAtAttackDistance(this))) return null; // Do not buy if ennemy too close

            var lessExpensivePotion = potions.OrderBy(x => x.ItemCost).FirstOrDefault();
            var lessExpensiveStuff = Items.OrderBy(x => x.ItemCost).FirstOrDefault();
            if (lessExpensivePotion != null && lessExpensiveStuff != null && PercentLife < 0.3)
            {
                if (lessExpensivePotion.ItemCost <= lessExpensiveStuff.ItemCost / 2)
                {
                    return Sell(lessExpensiveStuff);
                }
            }

            return null;
        }

        public string BuyStuff()
        {
            if (Player.Ennemy.Units.Any(x => x.IsAtAttackDistance(this))) return null; // Do not buy if engaged in combat

            // TODO : Improve : Buy armor ? Sell and rebuy
            var affordableItem = GetItemsOfInterest().FirstOrDefault(x => x.ItemCost <= Team.Gold);
            if (ItemsOwned < 3 && affordableItem != null && PercentLife > 0.5)
            {
                return Buy(affordableItem);
            }

            return null;
        }

        public string Buy(Item item)
        {
            Team.Gold -= item.ItemCost;

            if (!item.IsPotion)
                Items.Add(item);

            return $"BUY {item.ItemName}";
        }

        public string Sell(Item item)
        {
            Team.Gold += item.ItemCost / 2;

            if (!item.IsPotion)
                Items.Remove(item);

            return $"SELL {item.ItemName}";
        }

        public abstract bool CanCastSpell();

        public abstract IEnumerable<Unit> GetSpellTargets();

        public abstract string CastSpell();

        public static Hero GetHero(string[] inputs)
        {
            // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
            switch (inputs[19])
            {
                case "VALKYRIE":
                    return new Valkyrie(inputs);
                case "IRONMAN":
                    return new Ironman(inputs);
                case "DEADPOOL":
                    return new Deadpool(inputs);
                default:
                    return new BaseHero(inputs);
            }
        }

        public string BackToTower()
        {
            if (XY != Team.Units.OfType<Tower>().Single().Behind().XY)
                return MoveOrAttack(Team.Units.OfType<Tower>().Single().Behind());
            return "WAIT";
        }

        public string BackToBush()
        {
            var safeBushes =
                Player.Bushes.Where(x => !Player.Ennemy.Units.OfType<Tower>().Any(t => t.IsAtAttackDistance(x)) && !Player.Neutral.Units.Any(g => g.Distance(x) < 300)).ToList();
            var bush = safeBushes.OrderBy(x => x.Distance(this)).First();

            // TODO : attack ennemy hero if hidden. Doesn't work, cause my hero to go out of bush to charger an ennemy hero and be killed
            var ennemyHeroes = Player.Ennemy.Units.OfType<Hero>().Where(x => IsAtAttackDistance(x) && x.Distance(bush) < bush.Radius).ToList();

            if (ennemyHeroes.Any())
                return MoveOrAttack(ennemyHeroes.First());

            return $"MOVE {bush.XY}; HIDE";
        }

        public string BackSafe()
        {
            var backPosition = Player.Map.Where(x => Distance(x.Key) < MovementSpeed).OrderBy(x => x.Value).FirstOrDefault();

            return MoveOrAttack(backPosition.Key);
        }

        public string Logic()
        {
            // TODO : Improve : Move out of range, not necessarily to tower. 
            //Console.Error.WriteLine($"Has been hit : {HasBeenHitLastTurn}, close ennemy units : {Player.Ennemy.Units.Any(x => x.IsAtAttackDistance(this))}");
            if (HasBeenHitLastTurn && Player.Ennemy.Units.Any(x => x.IsAtAttackDistance(this)))
                return BackToBush();

            var ennemyTower = Player.Ennemy.Units.OfType<Tower>().Single();
            if (ennemyTower.IsAtAttackDistance(this) && Team.Units.Count(x => ennemyTower.IsAtAttackDistance(x)) <= 1) // No allied units under tower, go back !
                return BackToBush();

            // Ennemy can be at range this turn
            if (Player.Ennemy.Units.OfType<Hero>().Any(x => x.IsAtMoveAttackDistance(this, this)) && IsVisible)
                return BackToBush();

            if (!IsVisible)
            {
                var sellStuffBeforeDying = SellStuffBeforeDying();
                if (sellStuffBeforeDying != null) return sellStuffBeforeDying;

                var buyHealthPotion = BuyHealthPotion();
                if (buyHealthPotion != null) return buyHealthPotion;
            }

            if (PercentLife < 0.3 && Player.Ennemy.Units.OfType<Hero>().Any(x => x.Distance(this) < 400))
                return BackToBush();

            var buyStuff = BuyStuff();
            if (buyStuff != null) return buyStuff;

            //var nearestUnits = Player.Ennemy.Units.OrderBy(Distance).ToList();
            //Console.Error.WriteLine($"Hero {hero.UnitId} pos : {hero.X} {hero.Y}");
            //Console.Error.WriteLine($"Hero {hero.UnitId} has been hit last turn : {hero.HasBeenHitLastTurn}");
            //Console.Error.WriteLine($"Hero {hero.UnitId} health : {hero.Health}, previous : {hero.PreviousTurn?.Health}");
            //foreach (var unit in nearestUnits)
            //{
            //    Console.Error.WriteLine($"Nearest unit pos : {unit.X} {unit.Y}");
            //    Console.Error.WriteLine($"Nearest unit movement speed : {unit.MovementSpeed}");
            //    Console.Error.WriteLine($"Nearest unit attack range : {unit.AttackRange}");
            //    Console.Error.WriteLine($"Distance : {hero.Distance(unit)}");
            //    Console.Error.WriteLine($"IsAtAttackDistance : {unit.IsAtAttackDistance(hero)}");
            //}

            // Attack 
            var unitsToAttack = Player.GetUnitsWithValues(this).Where(x => x.Value > 0 && IsAtAttackDistance(x.Unit)).ToList();
            //foreach (var unit in unitsToAttack)
            //{
            //    Console.Error.WriteLine($"Unit {unit.Unit.UnitId} has value of {unit.Value}");
            //}

            var unitToAttack = unitsToAttack.FirstOrDefault()?.Unit;
            if (unitToAttack != null)
            {
                return $"ATTACK {unitToAttack.UnitId}";
            }

            var castSpell = CastSpell();
            if (castSpell != null) return castSpell;

            // TODO : Stay behind creeps
            var alliedTower = Team.Units.OfType<Tower>().Single();
            var privateRyan = Team.Units.OfType<Creep>().OrderBy(x => ennemyTower.Distance(x)).FirstOrDefault();
            if (privateRyan == null)
            {
                // No more allied creeps
                return BackToTower();
            }

            if (privateRyan.Distance(ennemyTower) < Distance(ennemyTower))
            {
                // Hero is behind creeps
                return MoveOrAttack(privateRyan.Behind());
            }

            return MoveOrAttack(privateRyan.Behind());

            if (PercentLife < 0.3)
            {
                return BackToTower();
            }

            // Nothing at distance, move towards closer ennemy
            var unit = Player.Units.Where(x => x.Team != Team).OrderBy(Distance).First();
            return $"ATTACK_NEAREST {unit.UnitType}";
        }

        private string MoveOrAttack(Position position)
        {
            var attackableUnit = Player.Ennemy.Units.Where(x => IsAtMoveAttackDistance(position, x))
                .Select(x => new UnitWithValue(x, x.GetValueForHero(this))).Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value).FirstOrDefault();

            return attackableUnit != null
                ? $"MOVE_ATTACK {position.XY} {attackableUnit.Unit.UnitId}"
                : $"MOVE {position.XY}";
        }
    }

    public class Tower : Unit
    {
        public Tower(string[] inputs) : base(inputs)
        {
        }
    }

    public class Groot : Unit
    {
        public Groot(string[] inputs) : base(inputs)
        {
        }
    }

    public class Item
    {
        public string ItemName;
        public int ItemCost;
        public int Damage;
        public int Health;
        public int MaxHealth;
        public int Mana;
        public int MaxMana;
        public int MoveSpeed;
        public int ManaRegeneration;
        public bool IsPotion;

        public Item(string[] inputs)
        {
            ItemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
            ItemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
            Damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
            Health = int.Parse(inputs[3]);
            MaxHealth = int.Parse(inputs[4]);
            Mana = int.Parse(inputs[5]);
            MaxMana = int.Parse(inputs[6]);
            MoveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
            ManaRegeneration = int.Parse(inputs[8]);
            IsPotion = int.Parse(inputs[9]) == 1; // 0 if it's not instantly consumed
        }
    }

    public class Bush : Position
    {
        public string EntityType;
        public int Radius;

        public Bush(string[] inputs) : base(int.Parse(inputs[1]), int.Parse(inputs[2]))
        {
            EntityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            Radius = int.Parse(inputs[3]);
        }
    }

    public class Team
    {
        public int TeamId;
        public int Gold;

        public List<Unit> Units = new List<Unit>();

        public Team(int teamId)
        {
            TeamId = teamId;
        }
    }

    // Should not be used, not complete !
    public class BaseHero : Hero
    {
        public BaseHero(string[] inputs) : base(inputs)
        {
        }

        public override bool CanCastSpell()
        {
            return false;
        }

        public override IEnumerable<Unit> GetSpellTargets()
        {
            return new List<Hero>();
        }

        public override string CastSpell()
        {
            return null;
        }
    }

    public class Valkyrie : Hero
    {
        public Valkyrie(string[] inputs) : base(inputs)
        {
        }

        public override bool CanCastSpell()
        {
            return Mana >= 20 && CountDown1 <= 0;
        }

        public override IEnumerable<Unit> GetSpellTargets()
        {
            return Player.Ennemy.Units.OfType<Hero>().Where(x => Distance(x) <= 155).OrderBy(x => x.Health).ToList();
        }

        public override string CastSpell()
        {
            var closestEnnemyHero = GetSpellTargets().FirstOrDefault();
            if (CanCastSpell() && closestEnnemyHero != null)
            {
                return $"SPEARFLIP {closestEnnemyHero.UnitId}";
            }

            return null;
        }
    }

    public class Ironman : Hero
    {
        public Ironman(string[] inputs) : base(inputs)
        {
        }

        public override IEnumerable<Item> GetItemsOfInterest()
        {
            // TODO : How are named ManaRegen or MaxMana items ?
            return Player.Items.Where(x => x.MaxMana > 20).OrderByDescending(x => x.MaxMana);
        }

        public override bool CanCastSpell()
        {
            return Player.Ennemy.Units.Any(x => Distance(x) < 900 && CountDown2 <= 0 && Mana >= 60);
        }

        public override IEnumerable<Unit> GetSpellTargets()
        {
            var groots = Player.Neutral.Units.Where(x => x.Distance(this) < 900).OrderBy(x => x.Distance(Player.Ennemy.Units.OfType<Tower>().Single())).ToList();

            var results = new List<Unit>();
            foreach (var groot in groots)
            {
                if (Player.Ennemy.Units.OfType<Hero>().Any(x => x.Distance(groot) < 300))
                    results.Add(groot);
            }

            if (results.Any())
                return results;

           return Player.Ennemy.Units.OrderBy(Distance).ToList();
        }

        public override string CastSpell()
        {
            var nearestUnit = GetSpellTargets().First();
            if (CanCastSpell() && nearestUnit != null)
            {
                return $"FIREBALL {nearestUnit.XY}";
            }

            return null;
        }
    }

    public class Deadpool : Hero
    {
        public Deadpool(string[] inputs) : base(inputs)
        {
        }

        public override bool CanCastSpell()
        {
            return Mana >= 50 && CountDown2 == 0 && Player.Ennemy.Units.OfType<Hero>().Any(x => Distance(x) < 200);
        }

        public override IEnumerable<Unit> GetSpellTargets()
        {
            return Player.Ennemy.Units.OfType<Hero>().Where(x => Distance(x) < 200).OrderBy(x => x.Health);
        }

        public override string CastSpell()
        {
            var nearestUnit = GetSpellTargets().FirstOrDefault();
            if (CanCastSpell() && nearestUnit != null)
            {
                return $"WIRE {nearestUnit.XY}";
            }

            return null;
        }
    }
}