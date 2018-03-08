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
        private static readonly List<Bush> Bushes = new List<Bush>();
        private static readonly List<Item> Items = new List<Item>();
        private static List<Unit> _units;
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
                    unit = new Unit(inputs);
                    break;
                case "HERO":
                    unit = new Hero(inputs);
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

            _units.Add(unit);
            return unit;
        }

        private void LaunchGame()
        {
            string[] inputs;
            _myTeam = int.Parse(ReadLine(true));

            var bushAndSpawnPointCount = int.Parse(ReadLine(true)); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
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
                if (_units != null)
                    previousUnits.AddRange(_units);
                _units = new List<Unit>();
                var entityCount = int.Parse(ReadLine());
                for (var i = 0; i < entityCount; i++)
                {
                    inputs = ReadLine().Split(' ');
                    var unit = ReadUnits(inputs);
                    unit.PreviousTurn = previousUnits.SingleOrDefault(x => x.UnitId == unit.UnitId);

                    var team = _teams.Single(x => x.TeamId == unit.TeamId);
                    unit.Team = team;
                }
                _teams.ForEach(x => x.Units = _units.Where(u => u.Team == x).ToList());

                Console.Error.WriteLine(_debugInitInput + _debugInput);

                GameLogic();
            } while (_stringReader == null);
        }

        private void GameLogic()
        {
            // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
            // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"

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

            foreach (var myHero in Me.Units.OfType<Hero>())
            {
                HeroLogic(myHero);
            }
        }

        public void HeroLogic(Hero hero)
        {
            Console.Error.WriteLine($"--- {hero.HeroType} ---");

            Item affordableItem;
            // Buy health potion
            // TODO : Improve : buy less expensive, do not buy potions that heals too much, is 0.7 a good factor ?
            if (hero.Health / hero.MaxHealth < 0.7)
            {
                affordableItem = Items.OrderByDescending(x => x.Health).FirstOrDefault(x => x.IsPotion && x.Health > 0 && x.ItemCost <= hero.Team.Gold);
                if (affordableItem != null)
                {
                    Console.WriteLine($"BUY {affordableItem.ItemName}");
                    return;
                }
            }

            // Buying stuff
            // TODO : Improve : Buy armor ? Sell and rebuy
            affordableItem = Items.OrderByDescending(x => x.Damage).FirstOrDefault(x => x.Damage > 0 && x.ItemCost <= hero.Team.Gold);
            if (hero.ItemsOwned < 3 && affordableItem != null)
            {
                Console.WriteLine($"BUY {affordableItem.ItemName}");
                return;
            }

            var nearestUnits = Ennemy.Units.OrderBy(hero.Distance).ToList();
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

            // TODO : Improve : Move out of range, not necessarily to tower. 
            var nearestUnit = nearestUnits.First();
            if (hero.HasBeenHitLastTurn && nearestUnit.IsAtAttackDistance(hero))
            {
                Console.WriteLine($"MOVE {Me.Units.OfType<Tower>().Single().XY}");
                return;
            }

            // Attack 
            var unitsToAttack = _units.Select(x => new UnitWithValue(x, x.GetValueForHero(hero))).Where(x => x.Value > 0).OrderByDescending(x => x.Value).ToList();
            foreach(var unit in unitsToAttack)
            {
                //Console.Error.WriteLine($"Unit {unit.Unit.UnitId} has value of {unit.Value}");
            }
            var unitToAttack = unitsToAttack.FirstOrDefault();
            if (unitToAttack != null)
            {
                Console.WriteLine($"ATTACK {unitToAttack.Unit.UnitId}");
                return;
            }

            if (hero.HeroType == "VALKYRIE" && hero.Mana >= 50 && hero.IsAtAttackDistance(nearestUnit) && hero.CountDown3 == 0)
            {
                Console.WriteLine("POWERUP");
                return;
            }
            if (hero.HeroType == "IRONMAN" && hero.Mana >= 50 && hero.Distance(nearestUnit) < 500 && hero.CountDown2 == 0)
            {
                Console.WriteLine($"FIREBALL {nearestUnit.XY}");
                return;
            }

            Console.WriteLine("ATTACK_NEAREST UNIT");
        }
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
        public double X;
        public double Y;
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

        public int Distance(Unit unit)
        {
            return (int)Math.Sqrt(Sqr(unit.Y - Y) + Sqr(unit.X - X));
        }
    }

    public class Unit : Position
    {
        public int UnitId;
        public int TeamId;
        public int AttackRange;
        public int Health;
        public int MaxHealth;
        public int Shield;
        public int AttackDamage;
        public int MovementSpeed;
        public int StunDuration;
        public int GoldValue;

        public Unit PreviousTurn;

        public Unit(string[] inputs) : base(int.Parse(inputs[3]), int.Parse(inputs[4]))
        {
            UnitId = int.Parse(inputs[0]);
            TeamId = int.Parse(inputs[1]);
            AttackRange = int.Parse(inputs[5]);
            Health = int.Parse(inputs[6]);
            MaxHealth = int.Parse(inputs[7]);
            Shield = int.Parse(inputs[8]); // useful in bronze
            AttackDamage = int.Parse(inputs[9]);
            MovementSpeed = int.Parse(inputs[10]);
            StunDuration = int.Parse(inputs[11]); // useful in bronze
            GoldValue = int.Parse(inputs[12]);
        }

        public Team Team;

        public bool IsAtAttackDistance(Unit unit)
        {
            return AttackRange >= Distance(unit);
        }

        public bool HasBeenHitLastTurn => PreviousTurn != null && PreviousTurn.Health > Health;

        public double GetValueForHero(Hero hero)
        {
            if (GetType() == typeof(Hero) && Team == Player.Me) return 0;
            if (!hero.IsAtAttackDistance(this)) return 0;
            double value = 0;
            value += 1 - (Health / hero.AttackDamage); // Gives more value to wounded units
            if (GetType() == typeof(Unit)) value += 1; 
            if (GetType() == typeof(Hero)) value += 3; 
            if (GetType() == typeof(Tower)) value += 5; 
            if (Health / hero.AttackDamage < 1) value += 10; // Always try to kill units that can die this turn

            if (Team == Player.Ennemy) value += 1; // Prefer ennemy

            return value;
        }
    }

    public class Hero : Unit
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

        public Hero(string[] inputs) : base(inputs)
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
}