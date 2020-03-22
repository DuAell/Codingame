﻿using System;
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

        public static void Play(string initData = null, string turnData = null, string startPositionData = null)
        {
            var initDataInputProcessor = new InputProcessor(initData);
            var turnDataInputProcessor = new InputProcessor(turnData);
            Initialize(initDataInputProcessor);

            // Output starting position
            var startingPosition = Map.Tiles.Where(x => x.IsWater).OrderBy(x => Guid.NewGuid()).First().ToString();
            if (startPositionData != null)
                startingPosition = startPositionData;

            //startingPosition = "11 8";

            Console.WriteLine(startingPosition);

            while (true)
            {
                initDataInputProcessor.WriteDebugData();
                GameTurn(turnDataInputProcessor);
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

        public static void GameTurn(InputProcessor inputProcessor)
        {
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

            UpdateEnnemyPosition(opponentOrders);

            ShowMapOfPossibleEnnemyPositions();

            var outputs = new List<string>();
            if (Me.CanGo(Direction.North))
                outputs.Add("MOVE N TORPEDO");
            else if (Me.CanGo(Direction.Est))
                outputs.Add("MOVE E TORPEDO");
            else if (Me.CanGo(Direction.South))
                outputs.Add("MOVE S TORPEDO");
            else if (Me.CanGo(Direction.West))
                outputs.Add("MOVE W TORPEDO");
            else
            {
                outputs.Add("SURFACE");
                Me.TileInfos.ForEach(_ => _.HasBeenVisited = false);
            }

            var ennemyPossiblePositions = Ennemy.TileInfos.Where(_ => _.CanBeThere).ToList();

            if (torpedoCooldown <= 0 && ennemyPossiblePositions.Count < 10)
            {
                // No matter if I shot myself, I will kill him
                if (ennemyPossiblePositions.Count == 1 && Me.Life > 1 && Ennemy.Life <= 2 && ennemyPossiblePositions.First().Tile.Manhattan(Me) <= 4)
                {
                    outputs.Add($"TORPEDO {Ennemy.TileInfos.First(_ => _.CanBeThere).Tile}");
                }
                else
                {
                    var fireTarget = ennemyPossiblePositions.Where(_ => _.Tile.Manhattan(Me) <= 4 && _.Tile.Manhattan(Me) > 2).OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                    if (fireTarget != null)
                        outputs.Add($"TORPEDO {fireTarget.Tile}");
                }
            }

            Console.WriteLine(string.Join(" | ", outputs));
        }

        private static void ShowMapOfPossibleEnnemyPositions()
        {
            for (var row = 0; row <= Ennemy.TileInfos.Max(_ => _.Tile.Y); row++)
            {
                var line = string.Empty;
                for (var column = 0; column <= Ennemy.TileInfos.Max(_ => _.Tile.X); column++)
                {
                    var tileInfo = Ennemy.GetTileInfo(Map.Tiles.First(_ => _.X == column && _.Y == row));
                    if (!tileInfo.Tile.IsWater)
                        line += "x";
                    else if (tileInfo.CanBeThere)
                        line += "?";
                    else
                        line += ".";
                }

                Console.Error.WriteLine(line);
            }
        }

        private static void UpdateEnnemyPosition(string opponentOrders)
        {
            var orders = opponentOrders.Split('|').Select(_ => _.Trim()).ToList();

            // First, process TORPEDO
            var order = orders.FirstOrDefault(_ => _.StartsWith("TORPEDO "));
            if (order != null)
            {
                UpdateEnnemyPosition_Torpedo(order.Replace("TORPEDO ", string.Empty));
            }

            order = orders.FirstOrDefault(_ => _.StartsWith("MOVE "));
            if (order != null)
            {
                UpdateEnnemyPosition_MoveTheMap(GetDirection(order.Substring("MOVE ".Length, 1)));
            }
            
            Console.Error.WriteLine($"Ennemy can be in {Ennemy.TileInfos.Count(_ => _.CanBeThere)} positions");
        }

        private static Direction GetDirection(string letter)
        {
            switch (letter)
            {
                case "N":
                    return Direction.North;
                case "S":
                    return Direction.South;
                case "E":
                    return Direction.Est;
                case "W":
                    return Direction.West;
                default:
                    throw new ArgumentOutOfRangeException(nameof(letter));
            }
        }

        private static void UpdateEnnemyPosition_MoveTheMap(Direction direction)
        {
            Direction oppositeDirection = Direction.North; // Default stupid value
            switch (direction)
            {
                case Direction.North:
                    oppositeDirection = Direction.South;
                    break;
                case Direction.West:
                    oppositeDirection = Direction.Est;
                    break;
                case Direction.Est:
                    oppositeDirection = Direction.West;
                    break;
                case Direction.South:
                    oppositeDirection = Direction.North;
                    break;
            }

            var canBeThereToFalse = new List<TileInfo>();
            var canBeThereToTrue = new List<TileInfo>();

            // Update all tiles with an adjacent of opposite direction with CanBeThere false to false => If going north : Remove bottom line
            canBeThereToFalse = Ennemy.TileInfos.Where(_ =>
                _.CanBeThere &&
                Ennemy.GetTileInfo(Map.GetAdjacent(_.Tile, oppositeDirection))?.CanBeThere != true).ToList();

            // Update all tiles with an adjacent of opposite direction with CanBeThere true to true => If going north : Add upper line
            canBeThereToTrue = Ennemy.TileInfos.Where(_ => !_.CanBeThere &&
                                                           _.Tile.IsWater &&
                                                           Ennemy.GetTileInfo(Map.GetAdjacent(_.Tile, oppositeDirection))?.CanBeThere == true).ToList();
            
            canBeThereToFalse.ForEach(_ => _.CanBeThere = false);
            canBeThereToTrue.ForEach(_ => _.CanBeThere = true);
        }

        private static void UpdateEnnemyPosition_Torpedo(string position)
        {
            Ennemy.TileInfos.Where(_ => _.Tile.Manhattan(new Position(int.Parse(position.Split(' ')[0]), int.Parse(position.Split(' ')[1]))) > 4).ToList().ForEach(_ => _.CanBeThere = false);
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
            var expectedDestination = Game.Map.GetAdjacent(this, direction);
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

        public Tile GetAdjacent(Position position, Direction direction)
        {
            int xModifier;
            switch (direction)
            {
                case Direction.Est:
                    xModifier = 1;
                    break;
                case Direction.West:
                    xModifier = -1;
                    break;
                default:
                    xModifier = 0;
                    break;
            }

            int yModifier;
            switch (direction)
            {
                case Direction.South:
                    yModifier = 1;
                    break;
                case Direction.North:
                    yModifier = -1;
                    break;
                default:
                    yModifier = 0;
                    break;
            }

            return Tiles.FirstOrDefault(_ => _.X == position.X + xModifier && _.Y == position.Y + yModifier);
        }
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

        public bool CanBeThere { get; set; } = true;
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
