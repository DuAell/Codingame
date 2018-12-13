using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace XmasRush
{
    public enum TurnType
    {
        Push,
        Move
    }

    /**
     * Help the Christmas elves fetch presents in a magical labyrinth!
     **/
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Launch();
        }        
    }

    public class Game
    {
        public InputProcessor InputProcessor { get; } = new InputProcessor();

        public void Launch()
        {
            // game loop
            while (true)
            {
                LaunchTurn();
            }
        }

        public void LaunchTurn(string debugData = null)
        {
            var input = debugData ?? InputProcessor.ProcessConsoleInput();

            var turnData = InputProcessor.ProcessInput(input);

            if (turnData.TurnType == TurnType.Push)
                Push(turnData);
            else
                Move(turnData);
        }

        public void Move(TurnData turnData)
        {
            var myPosition = turnData.Me.Tile;
            var goalPosition = turnData.GetQuests(turnData.Me).First().Item.Tile;

            if (goalPosition == null || myPosition == goalPosition)
            {
                Console.WriteLine("PASS");
                return;
            }

            var paths = GetPath(myPosition, goalPosition, turnData);
            if (paths.Any())
                Console.WriteLine($"MOVE {string.Join(" ", paths.Select(x => x.Command))}");
            else
                Console.WriteLine("PASS");
        }

        public List<Path> GetAvailablePaths(Tile origin, TurnData turnData)
        {
            var uncheckedTiles = new List<Tile> { origin };
            var checkedTiles = new List<Tile>();
            var bestPaths = new List<Path>();
            var cost = 0;

            while (uncheckedTiles.Count > 0)
            {
                var tile = uncheckedTiles.First();

                uncheckedTiles.Remove(tile);
                checkedTiles.Add(tile);

                cost++;

                foreach (var path in tile.GetNextPositions(turnData.Tiles))
                {
                    var questTiles = turnData.GetQuests(turnData.Me).Select(x => x.Item.Tile).Where(x => x != null);
                    path.Score = questTiles.Any(x => x == path.Destination) ? 100 : 0;

                    // Prefer paths where we have more possible exits
                    path.Score += path.Destination.GetNextPositions(turnData.Tiles).Count();
                    //path.Score += 100 - questTiles.Min(x => path.Destination.ManhattanDistance(x));

                    path.Cost = cost;

                    var otherPathForSameDestination = bestPaths.SingleOrDefault(x => x.Destination == path.Destination);
                    if (otherPathForSameDestination == null)
                        bestPaths.Add(path);
                    else if (otherPathForSameDestination.Score < path.Score || otherPathForSameDestination.Cost > path.Cost)
                    {
                        bestPaths.Remove(otherPathForSameDestination);
                        bestPaths.Add(path);
                    }
                                       
                    if (!checkedTiles.Contains(path.Destination))
                        uncheckedTiles.Add(path.Destination);
                }                
            }

            return bestPaths;
        }

        public List<Path> GetPath(Tile origin, Tile destination, TurnData turnData)
        {
            var availablePaths = GetAvailablePaths(origin, turnData);
            var bestPath = availablePaths.OrderByDescending(x => x.Score).ThenBy(x => x.Cost).FirstOrDefault();

            if (bestPath == null || bestPath.Destination == origin)
                return new List<Path>();

            var paths = new List<Path> { bestPath };
            var pathOrigin = bestPath.Origin;
            while (pathOrigin != origin)
            {
                var path = availablePaths.Where(x => x.Destination == pathOrigin).OrderByDescending(x => x.Score).ThenBy(x => x.Cost).First();
                paths.Add(path);
                pathOrigin = path.Origin;
            }

            paths.Reverse();
            return paths;
        }

        public void Push(TurnData turnData)
        {
            var routes = new List<Route>();

            for (int x = 0; x < 7; x++)
            {
                var turnDataClone = turnData.Clone();
                PushX(turnDataClone, x, Direction.Down);
                var route = GetRoute(turnDataClone);
                if (route != null)
                {
                    route.Command = $"{x} DOWN";
                    routes.Add(route);
                }
            }

            for (int x = 0; x < 7; x++)
            {
                var turnDataClone = turnData.Clone();
                PushX(turnDataClone, x, Direction.Up);
                var route = GetRoute(turnDataClone);
                if (route != null)
                {
                    route.Command = $"{x} UP";
                    routes.Add(route);
                }                    
            }

            for (int y = 0; y < 7; y++)
            {
                var turnDataClone = turnData.Clone();
                PushY(turnDataClone, y, Direction.Right);
                var route = GetRoute(turnDataClone);
                if (route != null)
                {
                    route.Command = $"{y} RIGHT";
                    routes.Add(route);
                }
            }

            for (int y = 0; y < 7; y++)
            {
                var turnDataClone = turnData.Clone();
                PushY(turnDataClone, y, Direction.Left);
                var route = GetRoute(turnDataClone);
                if (route != null)
                {
                    route.Command = $"{y} LEFT";
                    routes.Add(route);
                }
            }

            var bestRoute = GetBestRoute(routes);
            if (bestRoute != null)
                Console.WriteLine($"PUSH {bestRoute.Command}");
            else
                Console.WriteLine("PUSH 3 RIGHT"); // PUSH <id> <direction> | MOVE <direction> | PASS
        }

        private Route GetRoute(TurnData turnData)
        {
            var goalPosition = turnData.GetQuests(turnData.Me).First().Item.Tile;
            if (goalPosition == null)
                return null;
            return new Route
            {
                Paths = GetPath(turnData.Me.Tile, goalPosition, turnData)
            };
        }

        private void PushX(TurnData turnData, int x, Direction direction)
        {
            var tilesToMove = new List<Tile>();
            var positions = turnData.Tiles.Keys.Where(_ => _.X == x).ToArray();

            foreach (var position in positions)
            {
                var tile = turnData.Tiles[position];
                tilesToMove.Add(tile);
                turnData.Tiles.Remove(tile.Position);
            }

            foreach (var tile in tilesToMove)
            {
                tile.Position = new Position(tile.Position.X, (direction == Direction.Down ? tile.Position.Y + 1 : tile.Position.Y - 1));
                turnData.Tiles.Add(tile.Position, tile);

                if (tile.Position.Y == (direction == Direction.Down ? 7 : -1) && turnData.Me.Tile == tile)
                    turnData.Me.Tile = turnData.Me.TileInHand;
            }

            turnData.Me.TileInHand.Position = new Position(x, direction == Direction.Down ? 0 : 7);
            turnData.Tiles.Add(turnData.Me.TileInHand.Position, turnData.Me.TileInHand);
        }

        private void PushY(TurnData turnData, int y, Direction direction)
        {
            var tilesToMove = new List<Tile>();
            var positions = turnData.Tiles.Keys.Where(_ => _.Y == y).ToArray();
            foreach (var position in positions)
            {
                var tile = turnData.Tiles[position];
                tilesToMove.Add(tile);
                turnData.Tiles.Remove(position);
            }

            foreach (var tile in tilesToMove)
            {
                tile.Position = new Position((direction == Direction.Right ? tile.Position.X + 1 : tile.Position.X - 1), tile.Position.Y);
                turnData.Tiles.Add(tile.Position, tile);

                if (tile.Position.X == (direction == Direction.Right ? 7 : -1) && turnData.Me.Tile == tile)
                    turnData.Me.Tile = turnData.Me.TileInHand;
            }

            turnData.Me.TileInHand.Position = new Position(direction == Direction.Right ? 0 : 7, y);
            turnData.Tiles.Add(turnData.Me.TileInHand.Position, turnData.Me.TileInHand);
        }

        private Route GetBestRoute(List<Route> routes)
        {
            return routes.OrderByDescending(x => x.Score).ThenBy(x => x.Cost).FirstOrDefault();
        }
    }

    #region Data

    public class TurnData
    {
        public TurnType TurnType { get; set; }

        public List<Player> Players { get; private set; } = new List<Player>();

        public Player Me => Players.Single(x => x.Id == 0);

        public Player Opponent => Players.Single(x => x.Id == 1);

        public List<Item> Items { get; private set; } = new List<Item>();

        public List<Quest> Quests { get; private set; } = new List<Quest>();
        //public List<Tile> Tiles { get; private set; } = new List<Tile>();
        public Dictionary<Position, Tile> Tiles { get; set; } = new Dictionary<Position, Tile>();

        public IEnumerable<Quest> GetQuests(Player player)
        {
            return Quests.Where(x => x.Owner == player);
        }

        public TurnData Clone()
        {
            var clone = (TurnData)MemberwiseClone();
            clone.Tiles = new Dictionary<Position, Tile>();
            foreach (var tile in Tiles)
            {
                clone.Tiles.Add(tile.Key, tile.Value.Clone());
            }
            clone.Players = Players.Select(_ => _.Clone()).ToList();
            clone.Players.ForEach(x => x.Tile = clone.Tiles[x.Tile.Position]);

            clone.Items = Items.Select(_ => _.Clone()).ToList();
            clone.Items.ForEach(x => x.Tile = x.Tile == null ? null : clone.Tiles.ContainsKey(x.Tile.Position) ? clone.Tiles[x.Tile.Position] : null);
            clone.Items.ForEach(x => x.Owner = clone.Players.Single(_ => _.Id == x.Owner.Id));

            clone.Quests = Quests.Select(_ => _.Clone()).ToList();
            clone.Quests.ForEach(x => x.Owner = clone.Players.Single(_ => _.Id == x.Owner.Id));
            clone.Quests.ForEach(x => x.Item = clone.Items.Single(_ => _.Name == x.ItemName && _.Owner.Id == x.Owner.Id));

            return clone;
        }
    }

    public class Quest
    {
        public Quest(string itemName)
        {
            ItemName = itemName;
        }

        public Player Owner { get; set; }
        public string ItemName { get; }

        public Item Item { get; set; }

        public Quest Clone()
        {
            return (Quest)MemberwiseClone();
        }
    }

    public struct Position
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

        public int EuclidianDistance(Position position)
        {
            return (int)Math.Sqrt(Sqr(position.Y - Y) + Sqr(position.X - X));
        }

        public int ManhattanDistance(Position position)
        {
            return Math.Abs(position.X - X) + Math.Abs(position.Y - Y);
        }
    }

    public class Player
    {
        public Player(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public Tile TileInHand { get; set; }

        public Tile Tile { get; set; }

        public Player Clone()
        {
            return (Player)MemberwiseClone();
        }
    }

    public class Item
    {
        public Item(string name)
        {
            Name = name;
        }

        public Player Owner { get; set; }
        public string Name { get; }
        public Tile Tile { get; set; }

        public Item Clone()
        {
            return (Item)MemberwiseClone();
        }
    }

    public class Tile
    {
        public Tile(Position position, string desc)
        {
            if (desc[0] == '1')
                Up = true;
            if (desc[1] == '1')
                Right = true;
            if (desc[2] == '1')
                Down = true;
            if (desc[3] == '1')
                Left = true;

            Position = position;
        }

        public Position Position { get; set; }  

        public bool Up { get; }
        public bool Right { get; }
        public bool Down { get; }
        public bool Left { get; }

        public bool HasDirection(Direction direction)
        {
            if (direction == Direction.Up && Up)
                return true;
            if (direction == Direction.Right && Right)
                return true;
            if (direction == Direction.Down && Down)
                return true;
            if (direction == Direction.Left && Left)
                return true;

            return false;
        }

        public Path GetNextPosition(Direction direction, Dictionary<Position, Tile> map)
        {
            if (!HasDirection(direction))
                return null;

            if (direction == Direction.Up)
            {
                var position = new Position(Position.X, Position.Y - 1);
                if (map.ContainsKey(position) && map[position].Down)
                    return new Path(this, map[position], "UP");
            }

            if (direction == Direction.Right)
            {
                var position = new Position(Position.X + 1, Position.Y);
                if (map.ContainsKey(position) && map[position].Left)
                    return new Path(this, map[position], "RIGHT");
            }

            if (direction == Direction.Down)
            {
                var position = new Position(Position.X, Position.Y + 1);
                if (map.ContainsKey(position) && map[position].Up)
                    return new Path(this, map[position], "DOWN");
            }

            if (direction == Direction.Left)
            {
                var position = new Position(Position.X - 1, Position.Y);
                if (map.ContainsKey(position) && map[position].Right)
                    return new Path(this, map[position], "LEFT");
            }

            return null;
        }

        public IEnumerable<Path> GetNextPositions(Dictionary<Position, Tile> map)
        {
            var availablePositions = new List<Path>
            {
                GetNextPosition(Direction.Up, map),
                GetNextPosition(Direction.Right, map),
                GetNextPosition(Direction.Down, map),
                GetNextPosition(Direction.Left, map)
            };

            return availablePositions.Where(x => x != null);
        }

        public Tile Clone()
        {
            return (Tile)MemberwiseClone();
        }
    }

    public class Path
    {
        public Path(Tile origin, Tile destination, string command)
        {
            Origin = origin;
            Destination = destination;
            Command = command;
        }

        public Tile Origin { get; set; }
        public Tile Destination { get; set; }
        public string Command { get; set; }
        public int Score { get; set; }
        public int Cost { get; set; }
    }

    public class Route
    {
        public List<Path> Paths { get; set; }

        public string Command { get; set; }

        public int Score => Paths.Sum(x => x.Score);

        public int Cost => Paths.Sum(x => x.Cost);
    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public class InputProcessor
    {
        public List<string> InputLines { get; } = new List<string>();

        public string ProcessConsoleInput()
        {
            InputLines.Clear();

            InputLines.Add(ReadLine());

            for (int i = 0; i < 7; i++)
                InputLines.Add(ReadLine());

            for (int i = 0; i < 2; i++)
                InputLines.Add(ReadLine());

            GetNumberOfLinesAndLoop();
            GetNumberOfLinesAndLoop();

            Console.Error.WriteLine(string.Join(@"\n", InputLines));

            return string.Join("\n", InputLines);
        }

        private void GetNumberOfLinesAndLoop()
        {
            var numLines = ReadLine();
            InputLines.Add(numLines);
            for (int i = 0; i < int.Parse(numLines); i++)
                InputLines.Add(ReadLine());
        }

        private string ReadLine()
        {
            var line = Console.ReadLine();
            //Console.Error.WriteLine(line);
            return line;
        }

        public TurnData ProcessInput(string input)
        {
            var turnData = new TurnData();

            string[] inputs;

            using (var stringReader = new StringReader(input))
            {
                turnData.TurnType = int.Parse(stringReader.ReadLine()) == 0 ? TurnType.Push : TurnType.Move;

                for (int i = 0; i < 7; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');
                    for (int j = 0; j < 7; j++)
                    {
                        string tile = inputs[j];
                        var position = new Position(j, i);
                        turnData.Tiles.Add(position, new Tile(position, tile));
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    turnData.Players.Add(
                        new Player(i)
                        {
                            TileInHand = new Tile(new Position(-1, -1), inputs[3]),
                            Tile = turnData.Tiles[new Position(int.Parse(inputs[1]), int.Parse(inputs[2]))]
                        }
                    );
                }
                int numItems = int.Parse(stringReader.ReadLine()); // the total number of items available on board and on player tiles
                for (int i = 0; i < numItems; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    var position = new Position(int.Parse(inputs[1]), int.Parse(inputs[2]));
                    // Note: Si un objet se trouve sur la tuile d'un joueur, itemX et itemY serons égaux à -1 pour ce joueur, et -2 pour son adversaire.
                    var item = new Item(inputs[0])
                    {
                        Owner = turnData.Players.SingleOrDefault(x => x.Id == int.Parse(inputs[3])),
                        Tile = turnData.Tiles.ContainsKey(position) ? turnData.Tiles[position] : null
                    };

                    turnData.Items.Add(item);
                }
                int numQuests = int.Parse(stringReader.ReadLine()); // the total number of revealed quests for both players
                for (int i = 0; i < numQuests; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    var forPlayer = turnData.Players.Single(x => x.Id == int.Parse(inputs[1]));

                    var quest = new Quest(inputs[0])
                    {
                        Owner = forPlayer,
                        Item = turnData.Items.SingleOrDefault(x => x.Name == inputs[0] && x.Owner == forPlayer)
                    };

                    turnData.Quests.Add(quest);
                }
            }

            return turnData;
        }
    }

    #endregion
}
