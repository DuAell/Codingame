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
            var checkedPaths = new List<Path>();
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

                    checkedPaths.Add(path);
                    if (!checkedTiles.Contains(path.Destination))
                        uncheckedTiles.Add(path.Destination);
                }                
            }

            return checkedPaths;
        }

        public List<Path> GetPath(Tile origin, Tile destination, TurnData turnData)
        {
            var availablePaths = GetAvailablePaths(origin, turnData);
            //var orderedPaths = availablePaths.OrderBy(x => x.Destination.ManhattanDistance(destination)).Select(x => new TileWithDistance { XY = x.Destination.XY, Distance = x.Destination.ManhattanDistance(destination) });
            //var closest = availablePaths.OrderBy(x => x.Destination.ManhattanDistance(destination)).FirstOrDefault();
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
            var subSet = turnData.Tiles.Where(_ => _.X == x).ToList();
            if (direction == Direction.Down)
                subSet.ForEach(_ => _.Y++);
            else
                subSet.ForEach(_ => _.Y--);

            turnData.Me.TileInHand.X = x;
            turnData.Me.TileInHand.Y = direction == Direction.Down ? 0 : 7;
            turnData.Tiles.Add(turnData.Me.TileInHand);
            var oldTile = turnData.Tiles.Single(_ => _.Y == (direction == Direction.Down ? 7 : -1));
            turnData.Tiles.Remove(oldTile);

            if (turnData.Me.Tile == oldTile)
                turnData.Me.Tile = turnData.Me.TileInHand;
        }

        private void PushY(TurnData turnData, int y, Direction direction)
        {
            var subSet = turnData.Tiles.Where(_ => _.Y == y).ToList();
            if (direction == Direction.Right)
                subSet.ForEach(_ => _.X++);
            else
                subSet.ForEach(_ => _.X--);

            turnData.Me.TileInHand.Y = y;
            turnData.Me.TileInHand.X = direction == Direction.Right ? 0 : 7;
            turnData.Tiles.Add(turnData.Me.TileInHand);
            var oldTile = turnData.Tiles.Single(_ => _.X == (direction == Direction.Right ? 7 : -1));
            turnData.Tiles.Remove(oldTile);

            if (turnData.Me.Tile == oldTile)
                turnData.Me.Tile = turnData.Me.TileInHand;
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
        public List<Tile> Tiles { get; private set; } = new List<Tile>();

        public IEnumerable<Quest> GetQuests(Player player)
        {
            return Quests.Where(x => x.Owner == player);
        }

        public TurnData Clone()
        {
            var clone = (TurnData)MemberwiseClone();
            clone.Tiles = Tiles.Select(_ => _.Clone()).ToList();
            clone.Players = Players.Select(_ => _.Clone()).ToList();
            clone.Players.ForEach(x => x.Tile = clone.Tiles.Single(_ => _.XY == x.Tile.XY));

            clone.Items = Items.Select(_ => _.Clone()).ToList();
            clone.Items.ForEach(x => x.Tile = clone.Tiles.SingleOrDefault(_ => _.XY == x.Tile?.XY));
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

        public int EuclidianDistance(Position position)
        {
            return (int)Math.Sqrt(Sqr(position.Y - Y) + Sqr(position.X - X));
        }

        public int ManhattanDistance(Position position)
        {
            return Math.Abs(Math.Abs(position.X - X) + Math.Abs(position.Y - Y));
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

    public class Tile : Position
    {
        public Tile(int x, int y, string desc) : base(x, y)
        {
            if (desc[0] == '1')
                Up = true;
            if (desc[1] == '1')
                Right = true;
            if (desc[2] == '1')
                Down = true;
            if (desc[3] == '1')
                Left = true;
        }

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

        public Path GetNextPosition(Direction direction, List<Tile> map)
        {
            if (!HasDirection(direction))
                return null;

            Tile nextTile = null;
            if (direction == Direction.Up)
            {
                nextTile = map.SingleOrDefault(x => x.X == X && x.Y == Y - 1);
                if (nextTile?.Down == true) return new Path(this, nextTile, "UP");
            }

            if (direction == Direction.Right)
            {
                nextTile = map.SingleOrDefault(x => x.X == X + 1 && x.Y == Y);
                if (nextTile?.Left == true) return new Path(this, nextTile, "RIGHT");
            }

            if (direction == Direction.Down)
            {
                nextTile = map.SingleOrDefault(x => x.X == X && x.Y == Y + 1);
                if (nextTile?.Up == true) return new Path(this, nextTile, "DOWN");
            }

            if (direction == Direction.Left)
            {
                nextTile = map.SingleOrDefault(x => x.X == X - 1 && x.Y == Y);
                if (nextTile?.Right == true) return new Path(this, nextTile, "LEFT");
            }

            return null;
        }

        public IEnumerable<Path> GetNextPositions(List<Tile> map)
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

                        turnData.Tiles.Add(new Tile(j, i, tile));
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    turnData.Players.Add(
                        new Player(i)
                        {
                            TileInHand = new Tile(-1, -1, inputs[3]),
                            Tile = turnData.Tiles.Single(_ => _.X == int.Parse(inputs[1]) && _.Y == int.Parse(inputs[2]))
                        }
                    );
                }
                int numItems = int.Parse(stringReader.ReadLine()); // the total number of items available on board and on player tiles
                for (int i = 0; i < numItems; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    // Note: Si un objet se trouve sur la tuile d'un joueur, itemX et itemY serons égaux à -1 pour ce joueur, et -2 pour son adversaire.
                    var item = new Item(inputs[0])
                    {
                        Owner = turnData.Players.SingleOrDefault(x => x.Id == int.Parse(inputs[3])),
                        Tile = turnData.Tiles.SingleOrDefault(_ => _.X == int.Parse(inputs[1]) && _.Y == int.Parse(inputs[2]))
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
