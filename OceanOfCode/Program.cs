using System;
using System.Linq;
using System.Collections.Generic;

namespace OceanOfCode
{

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class OceanOfCode
    {
        static void Main(string[] args)
        {
            Game.Play();
        }
    }

    public static class Game
    {
        public static Map Map { get; set; }

        public static Player Me { get; set; }

        public static Player Ennemy { get; set; }

        public static void Play(string initData = null, string turnData = null)
        {
            var initDataInputProcessor = new InputProcessor(initData);
            Initialize(initDataInputProcessor);

            // Output starting position
            var startingPosition = Map.Tiles.Where(x => x.IsWater).OrderBy(x => Guid.NewGuid()).First();
            startingPosition = Map.Tiles.First(_ => _.X == 4 && _.Y == 5);
            Console.WriteLine(startingPosition);

            while (true)
            {
                initDataInputProcessor.WriteDebugData();
                GameTurn(turnData);
            }
        }

        public static void Initialize(InputProcessor inputProcessor)
        {
            var inputs = inputProcessor.ReadLine().Split(' ');

            Map = new Map {Width = int.Parse(inputs[0]), Height = int.Parse(inputs[1])};

            for (var row = 0; row < Map.Height; row++)
            {
                var line = inputProcessor.ReadLine();
                for (var column = 0; column < Map.Width; column++)
                {
                    Map.Tiles.Add(new Tile(column, row) { IsWater = line[column] == '.' });
                }
            }

            Me = new Player(-1, -1)
            {
                Id = int.Parse(inputs[2]),
                TileInfos = new List<TileInfo>(Map.Tiles.Select(_ => new TileInfo(_)))
            };

            Ennemy = new Player(-1, -1)
            {
                TileInfos = new List<TileInfo>(Map.Tiles.Select(_ => new TileInfo(_)))
            };
        }

        public static void GameTurn(string turnData = null)
        {
            var inputProcessor = new InputProcessor(turnData);

            var inputs = inputProcessor.ReadLine().Split(' ');
            Me.X = int.Parse(inputs[0]);
            Me.Y = int.Parse(inputs[1]);
            Me.TileInfos.First(_ => _.Tile.X == Me.X && _.Tile.Y == Me.Y).HasBeenVisited = true;
            Me.Life = int.Parse(inputs[2]);
            Ennemy.Life = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = inputProcessor.ReadLine();
            string opponentOrders = inputProcessor.ReadLine();

            inputProcessor.WriteDebugData();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            if (Me.CanGo(Direction.North))
                Console.WriteLine("MOVE N TORPEDO");
            else if (Me.CanGo(Direction.Est))
                Console.WriteLine("MOVE E TORPEDO");
            else if (Me.CanGo(Direction.South))
                Console.WriteLine("MOVE S TORPEDO");
            else if (Me.CanGo(Direction.West))
                Console.WriteLine("MOVE W TORPEDO");
            else 
                Console.WriteLine("SURFACE");
        }
    }

    public class Player : Position
    {
        public Player(int x, int y) : base(x, y)
        {
        }

        public int Id { get; set; }
        public List<TileInfo> TileInfos { get; set; } = new List<TileInfo>();

        public TileInfo GetTileInfo(Tile tile)
            => TileInfos.FirstOrDefault(_ => _.Tile == tile);

        public int Life { get; set; }

        public bool CanGo(Direction direction)
        {
            var xModifier = direction == Direction.Est ? 1 : direction == Direction.West ? -1 : 0;
            var yModifier = direction == Direction.South ? 1 : direction == Direction.North ? -1 : 0;

            var expectedDestination = Game.Map.Tiles.FirstOrDefault(_ => _.X == X + xModifier && _.Y == Y + yModifier);
            var result = expectedDestination?.IsWater == true && !GetTileInfo(expectedDestination).HasBeenVisited;

            Console.Error.WriteLine($"{(result ? "Can" : "Cannot")} go {direction} ({expectedDestination}). Water: {expectedDestination?.IsWater} / Already visited: {GetTileInfo(expectedDestination)?.HasBeenVisited}");

            return result;
        }
    }

    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<Tile> Tiles { get; set; } = new List<Tile>();
    }

    public class TileInfo
    {
        public Tile Tile { get; }

        public TileInfo(Tile tile)
        {
            Tile = tile;
            if (!tile.IsWater)
                CanBeThere = false;
        }

        public bool HasBeenVisited { get; set; }

        public bool CanBeThere { get; set; }
    }

    public enum Direction
    {
        North,
        South,
        Est,
        West
    }

    public class Tile : Position
    {
        public Tile(int x, int y) : base(x, y)
        {
        }

        public bool IsWater { get; set; }
    }

    public class Position
    {
        public int X, Y;
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
}
