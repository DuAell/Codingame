using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeALaMode
{
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

    public class Program
    {
        public class Game
        {
            public Player[] Players = new Player[2];
            public Table Dishwasher;
            public Table Window;
            public Table Blueberry;
            public Table Strawberry;
            public Table IceCream;
            public Table ChoppingBoard;
            public List<Table> Tables = new List<Table>();
        }

        public class Table
        {
            public Position Position;
            public bool HasFunction;
            public Item Item;
        }

        public class Item
        {
            public string Content;
            public bool HasPlate;
            public Item(string content)
            {
                Content = content;
                HasPlate = Content.Contains(MainClass.Dish);
            }
        }

        public class Player
        {
            public Position Position;
            public Item Item;
            public Player(Position position, Item item)
            {
                Position = position;
                Item = item;
            }
            public void Update(Position position, Item item)
            {
                Position = position;
                Item = item;
            }
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

        public class MainClass
        {
            public static string GameData { get; set; }
            public static string TurnData { get; set; }

            public static bool Debug = true;
            public const string Dish = "DISH";

            public static Game ReadGame()
            {
                var inputProcessor = new InputProcessor(GameData);

                string[] inputs;

                // ALL CUSTOMERS INPUT: to ignore until Bronze
                int numAllCustomers = int.Parse(inputProcessor.ReadLine());
                for (int i = 0; i < numAllCustomers; i++)
                {
                    inputs = inputProcessor.ReadLine().Split(' ');
                    string customerItem = inputs[0]; // the food the customer is waiting for
                    int customerAward = int.Parse(inputs[1]); // the number of points awarded for delivering the food
                }

                var game = new Game();
                game.Players[0] = new Player(null, null);
                game.Players[1] = new Player(null, null);

                for (int i = 0; i < 7; i++)
                {
                    string kitchenLine = inputProcessor.ReadLine();
                    for (var x = 0; x < kitchenLine.Length; x++)
                    {
                        if (kitchenLine[x] == 'W') game.Window = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == 'D') game.Dishwasher = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == 'I') game.IceCream = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == 'B') game.Blueberry = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == 'C') game.ChoppingBoard = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == 'S') game.Strawberry = new Table { Position = new Position(x, i), HasFunction = true };
                        if (kitchenLine[x] == '#') game.Tables.Add(new Table { Position = new Position(x, i) });
                    }
                }

                inputProcessor.WriteDebugData();

                return game;
            }

            private static void Move(Position p) => Console.WriteLine("MOVE " + p);

            private static void Use(Position p, string comment = "")
            {
                Console.WriteLine("USE " + p + "; "+ comment);
            }

            public static Game Game;
            public static Player MyChef;

            public static void Main()
            {
                string[] inputs;

                Game = ReadGame();

                MyChef = Game.Players[0];

                while (true)
                {
                    var inputProcessor = new InputProcessor(TurnData);

                    var gameTurn = new GameTurn();
                    int turnsRemaining = int.Parse(inputProcessor.ReadLine());

                    // PLAYERS INPUT
                    inputs = inputProcessor.ReadLine().Split(' ');
                    Game.Players[0].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), new Item(inputs[2]));
                    inputs = inputProcessor.ReadLine().Split(' ');
                    Game.Players[1].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), new Item(inputs[2]));

                    //Clean other tables
                    foreach (var t in Game.Tables)
                    {
                        t.Item = null;
                    }
                    int numTablesWithItems = int.Parse(inputProcessor.ReadLine()); // the number of tables in the kitchen that currently hold an item
                    for (int i = 0; i < numTablesWithItems; i++)
                    {
                        inputs = inputProcessor.ReadLine().Split(' ');
                        var table = Game.Tables.First(t => t.Position.X == int.Parse(inputs[0]) && t.Position.Y == int.Parse(inputs[1]));
                        table.Item = new Item(inputs[2]);
                    }

                    inputs = inputProcessor.ReadLine().Split(' ');
                    string ovenContents = inputs[0]; // ignore until bronze league
                    int ovenTimer = int.Parse(inputs[1]);
                    int numCustomers = int.Parse(inputProcessor.ReadLine()); // the number of customers currently waiting for food
                    for (int i = 0; i < numCustomers; i++)
                    {
                        inputs = inputProcessor.ReadLine().Split(' ');
                        string customerItem = inputs[0];
                        int customerAward = int.Parse(inputs[1]);
                    }

                    inputProcessor.WriteDebugData();

                    var tablesNotEmpty = Game.Tables.Where(x => x.Item != null).ToList();

                    // GAME LOGIC
                    if (!MyChef.Item?.HasPlate ?? false)
                        gameTurn.PossibleMoves.Add(new PossibleMove{ Name = "Dishwasher", Position = Game.Dishwasher.Position, Score = 0});
                    if (MyChef.Item.Content.Count(x => x == '-') >= 3)
                        gameTurn.PossibleMoves.Add(new PossibleMove { Name = "Window", Position = Game.Window.Position, Score = 0 });
                    if (!MyChef.Item.Content.Contains(Ingredient.IceCream))
                    {
                        gameTurn.AddPossibleMove(new PossibleMove { Position = Game.IceCream.Position, Name = Ingredient.IceCream });
                    }
                    if (!MyChef.Item.Content.Contains(Ingredient.Blueberries))
                    {
                        gameTurn.AddPossibleMove(new PossibleMove { Position = Game.Blueberry.Position, Name = Ingredient.Blueberries });
                    }
                    if (!MyChef.Item.Content.Contains(Ingredient.ChoppedStrawberries) && tablesNotEmpty.Any(x => x.Item.Content.Contains(Ingredient.ChoppedStrawberries)))
                    {
                        gameTurn.AddPossibleMove(new PossibleMove { Position = tablesNotEmpty.First(x => x.Item.Content.Contains(Ingredient.ChoppedStrawberries)).Position, Name = Ingredient.ChoppedStrawberries });
                    }
                    gameTurn.PossibleMoves.Add(new PossibleMove { Name = "Window", Position = Game.Window.Position, Score = 100 });

                    if (gameTurn.PossibleMoves.Any())
                    {
                        var chosenMove = gameTurn.PossibleMoves.OrderBy(x => x.Score).First();
                        Use(chosenMove.Position, $"{chosenMove.Name}: {chosenMove.Score}");
                    }
                    else
                        Console.WriteLine("WAIT; Stuck !");
                    
                    // once ready, put it on the first empty table for now
                    //Use(game.Tables.First(t => t.Item == null).Position);

                    var recipes = new List<Recipe>
                    {
                        new Recipe
                        {
                            Name = "DISH-ICE_CREAM",
                            Ingredients = new List<Ingredient>
                            {
                                new Ingredient {Name = Dish, Order = 0},
                                new Ingredient {Name = "ICE_CREAM", Order = 0}
                            }
                        }
                    };

                    if (inputProcessor.IsDebug)
                        return;
                }
            }

            public class GameTurn
            {
                public readonly List<PossibleMove> PossibleMoves = new List<PossibleMove>();

                public void AddPossibleMove(PossibleMove possibleMove)
                {
                    possibleMove.Score = MyChef.Position.Manhattan(possibleMove.Position);
                    PossibleMoves.Add(possibleMove);
                }
            }
        }

        public class PossibleMove
        {
            public string Name { get; set; }
            public Position Position { get; set; }
            public int Score { get; set; }
        }

        public class Recipe
        {
            public string Name { get; set; }

            public List<Ingredient> Ingredients { get; set; }
        }

        public class Ingredient
        {
            public const string Dish = "DISH";
            public const string IceCream = "ICE_CREAM";
            public const string Blueberries = "BLUEBERRIES";
            public const string Strawberries = "STRAWBERRIES";
            public const string ChoppedStrawberries = "CHOPPED_STRAWBERRIES";

            public string Name { get; set; }

            public int Order { get; set; }
        }
    }
}
