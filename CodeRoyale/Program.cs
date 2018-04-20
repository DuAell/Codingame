using System;
using System.Linq;
using System.Collections.Generic;

namespace CodeRoyale
{
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();

            game.Init();
        }

        public static string ConsoleEntryDebug { get; set; } = string.Empty;

        public static string ConsoleReadLine()
        {
            var data = Console.ReadLine();
            ConsoleEntryDebug += $@"{data}\n";
            return data;
        }
    }

    public class Game
    {
        public List<Unit> Units { get; set; }

        public List<Site> Sites { get; set; } = new List<Site>();

        public Gamer Me = new Gamer();

        public Gamer Opponent = new Gamer();

        public void Init()
        {
            string[] inputs;
            var numSites = int.Parse(Player.ConsoleReadLine());
            for (var i = 0; i < numSites; i++)
            {
                inputs = Player.ConsoleReadLine().Split(' ');
                Sites.Add(new Site(inputs));
            }

            // game loop
            while (true)
            {
                Units = new List<Unit>();

                inputs = Player.ConsoleReadLine().Split(' ');
                Me.Update(inputs, this);

                for (var i = 0; i < numSites; i++)
                {
                    inputs = Player.ConsoleReadLine().Split(' ');
                    var siteId = int.Parse(inputs[0]);
                    Sites.Single(x => x.SiteId == siteId).Update(inputs);
                }

                var numUnits = int.Parse(Player.ConsoleReadLine());
                for (var i = 0; i < numUnits; i++)
                {
                    Units.Add(Unit.Create(Player.ConsoleReadLine().Split(' ')));
                }

                Console.Error.WriteLine(Player.ConsoleEntryDebug);

                GameLogic();

                Player.ConsoleEntryDebug = string.Empty;
            }
        }

        public void GameLogic()
        {
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // First line: A valid queen action
            // Second line: A set of training instructions
            QueenLogic();
            TrainingLogic();
        }

        public void QueenLogic()
        {
            if (Me.TouchedSite != null && Me.TouchedSite.Owner == Owner.None)
                Console.WriteLine($"BUILD {Me.TouchedSite.SiteId} BARRACKS-KNIGHT");
            else
                Console.WriteLine("WAIT");
        }

        public void TrainingLogic()
        {
            var mySites = Sites.Where(x => x.Owner == Owner.Me && x.TurnsLeftBeforeBuilding == 0).ToList();

            var knightBarracks = mySites.Where(x => x.ProductionType == typeof(Knight)).Take(Me.Gold / Knight.Cost);

            if (!knightBarracks.Any())
                Console.WriteLine("TRAIN");
            else
                Console.WriteLine($"TRAIN {string.Join(" ", knightBarracks.Select(x => x.SiteId))}");
        }
    }

    public class Gamer
    {
        public int Gold { get; private set; }
        public Site TouchedSite { get; private set; }

        public void Update(string[] inputs, Game game)
        {
            Gold = int.Parse(inputs[0]);
            TouchedSite = game.Sites.SingleOrDefault(x => x.SiteId == int.Parse(inputs[1])); // -1 if none
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

    public enum StructureType
    {
        None,
        Barracks
    }

    public enum Owner
    {
        None,
        Me,
        Ennemy
    }

    public enum ProductionType
    {
        None,
        Knight,
        Archer
    }

    public class Site : Position
    {
        public int SiteId { get; }

        public int Radius { get; }

        public StructureType StructureType { get; private set; }

        public Owner Owner { get; private set; }

        public int TurnsLeftBeforeBuilding { get; private set; }

        public Type ProductionType { get; private set; }

        public Site(string[] inputs) : base(int.Parse(inputs[1]), int.Parse(inputs[2]))
        {
            SiteId = int.Parse(inputs[0]);
            Radius = int.Parse(inputs[3]);
        }

        public void Update(string[] inputs)
        {
            //int ignore1 = int.Parse(inputs[1]); // used in future leagues
            //int ignore2 = int.Parse(inputs[2]); // used in future leagues

            switch (int.Parse(inputs[3]))
            {
                case -1:
                    StructureType = StructureType.None;
                    break;
                case 2:
                    StructureType = StructureType.Barracks;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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

            TurnsLeftBeforeBuilding = int.Parse(inputs[5]);

            switch (int.Parse(inputs[6]))
            {
                case -1:
                    ProductionType = null;
                    break;
                case 0:
                    ProductionType = typeof(Knight);
                    break;
                case 1:
                    ProductionType = typeof(Archer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
}