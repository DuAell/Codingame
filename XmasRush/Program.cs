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
            var myPosition = turnData.Tiles.Single(x => x.XY == turnData.Me.XY);
            var goalPosition = turnData.Tiles.SingleOrDefault(x => x.XY == turnData.Me.Quests.First().Item.XY);

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

            while (uncheckedTiles.Count > 0)
            {
                var tile = uncheckedTiles.First();
                uncheckedTiles.Remove(tile);
                checkedTiles.Add(tile);

                foreach (var path in tile.GetNextPositions(turnData.Tiles))
                {
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
            var closest = availablePaths.OrderBy(x => x.Destination.Distance(destination)).FirstOrDefault();
            if (closest == null || closest.Destination == origin)
                return new List<Path>();

            var paths = new List<Path> { closest };
            var pathOrigin = closest.Origin;
            while (pathOrigin != origin)
            {
                var path = availablePaths.First(x => x.Destination == pathOrigin);
                paths.Add(path);
                pathOrigin = path.Origin;
            }

            paths.Reverse();
            return paths;
        }

        public void Push(TurnData turnData)
        {
            Console.WriteLine("PUSH 3 RIGHT"); // PUSH <id> <direction> | MOVE <direction> | PASS
        }
    }

    #region Data

    public class TurnData
    {
        public TurnType TurnType { get; set; }

        public List<Player> Players { get; } = new List<Player>();

        public Player Me => Players.Single(x => x.Id == 0);

        public Player Opponent => Players.Single(x => x.Id == 1);

        public List<Item> Items { get; } = new List<Item>();

        public List<Quest> Quests { get; } = new List<Quest>();
        public List<Tile> Tiles { get; } = new List<Tile>();
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
    }

    public class Player : Position
    {
        public Player(int x, int y, int id) : base(x, y)
        {
            Id = id;
        }

        public int Id { get; set; }

        public List<Quest> Quests { get; } = new List<Quest>();

        public Tile Tile { get; set; }
    }

    public class Item : Position
    {
        public Item(int x, int y, string name) : base(x, y)
        {
            Name = name;
        }

        public Player Owner { get; set; }
        public string Name { get; }
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
        public string Path { get; set; }

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
                        new Player(int.Parse(inputs[1]), int.Parse(inputs[2]), i)
                        {
                            Tile = new Tile(-1, -1, inputs[3])
                        }
                    );
                }
                int numItems = int.Parse(stringReader.ReadLine()); // the total number of items available on board and on player tiles
                for (int i = 0; i < numItems; i++)
                {
                    inputs = stringReader.ReadLine().Split(' ');

                    // Note: Si un objet se trouve sur la tuile d'un joueur, itemX et itemY serons égaux à -1 pour ce joueur, et -2 pour son adversaire.
                    var item = new Item(int.Parse(inputs[1]), int.Parse(inputs[2]), inputs[0])
                    {
                        Owner = turnData.Players.SingleOrDefault(x => x.Id == int.Parse(inputs[3]))
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

                    forPlayer.Quests.Add(quest);
                }
            }

            return turnData;
        }
    }

    #endregion
}
