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
            public List<Order> Orders = new List<Order>();

            public Player MyChef;
        }

        public class Order
        {
            public string Item;
            public int Award;
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

                var game = new Game();

                // ALL CUSTOMERS INPUT: to ignore until Bronze
                int numAllCustomers = int.Parse(inputProcessor.ReadLine());
                for (int i = 0; i < numAllCustomers; i++)
                {
                    inputs = inputProcessor.ReadLine().Split(' ');
                    game.Orders.Add(new Order
                    {
                        Item = inputs[0], // the food the customer is waiting for
                        Award = int.Parse(inputs[1]) // the number of points awarded for delivering the food
                    });
                }

                game.Players[0] = new Player(null, null);
                game.Players[1] = new Player(null, null);

                game.MyChef = game.Players[0];

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

            public static void Main()
            {
                string[] inputs;

                Game = ReadGame();

                var recipes = new List<Recipe>
                {
                    Recipe.DishIceCream,
                    Recipe.DishBlueberries,
                    Recipe.DishChoppedStrawberries
                };

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
                        gameTurn.Orders.Add(new Order
                        {
                            Item = inputs[0], // the food the customer is waiting for
                            Award = int.Parse(inputs[1]) // the number of points awarded for delivering the food
                        });
                    }

                    inputProcessor.WriteDebugData();

                    var tablesNotEmpty = Game.Tables.Where(x => x.Item != null).ToList();
                    var possibleMoves = new List<PossibleMove>();

                    // GAME LOGIC
                    if (!Game.MyChef.Item.HasPlate)
                        possibleMoves.Add(new PossibleMove{ Name = "Dishwasher", Position = Game.Dishwasher.Position, Score = 0});

                    if (gameTurn.Orders.Any(x => x.Item == Game.MyChef.Item.Content))
                        possibleMoves.Add(new PossibleMove { Name = "Window", Position = Game.Window.Position, Score = 0 });

                    foreach (var order in gameTurn.Orders.OrderByDescending(x => x.Award))
                    {
                        var recipe = recipes.FirstOrDefault(x => x.Name == order.Item);
                        if (recipe == null)
                        {
                            Console.Error.WriteLine($"Unknown recipe: {order.Item}");
                            continue;
                        }

                        possibleMoves.Add(recipe.GetBestMove(Game, gameTurn));


                        //var ingredients = order.Item.Split('-');
                        //foreach (var ingredient in ingredients)
                        //{
                        //    if (!Game.MyChef.Item.Content.Contains(ingredient))
                        //    {
                        //        var possibleMove = gameTurn.GetBestMoveForIngredient(ingredient);
                        //        if (possibleMove != null)
                        //            possibleMoves.Add(possibleMove);
                        //    }
                        //}
                    }

                    possibleMoves.Add(new PossibleMove { Name = "Window", Position = Game.Window.Position, Score = 10000 });

                    if (possibleMoves.Any())
                    {
                        var chosenMove = possibleMoves.OrderBy(x => x.Score).First();
                        Use(chosenMove.Position, $"{chosenMove.Name}: {chosenMove.Score}");
                    }
                    else
                        Console.WriteLine("WAIT; Stuck !");
                    
                    // once ready, put it on the first empty table for now
                    //Use(game.Tables.First(t => t.Item == null).Position);

                    if (inputProcessor.IsDebug)
                        return;
                }
            }

            public class GameTurn
            {
                public List<Order> Orders = new List<Order>();

                public PossibleMove GetBestMoveForIngredient(string ingredient)
                {
                    return GetPossibleMovesForIngredient(ingredient).OrderBy(x => x.Score).FirstOrDefault();
                }

                private List<PossibleMove> GetPossibleMovesForIngredient(string ingredient)
                {
                    var possibleMoves = new List<PossibleMove>();
                    PossibleMove possibleMove;

                    var tablesNotEmpty = Game.Tables.Where(x => x.Item != null).ToList();

                    switch (ingredient)
                    {
                        case IngredientName.IceCream:
                            possibleMove = new PossibleMove {Position = Game.IceCream.Position, Name = IngredientName.IceCream};
                            possibleMove.CalculateScoreFromPosition(Game.MyChef.Position);
                            possibleMoves.Add(possibleMove);
                            break;
                        case IngredientName.Blueberries:
                            possibleMove = new PossibleMove { Position = Game.Blueberry.Position, Name = IngredientName.Blueberries };
                            possibleMove.CalculateScoreFromPosition(Game.MyChef.Position);
                            possibleMoves.Add(possibleMove);
                            break;
                        case IngredientName.ChoppedStrawberries:
                            possibleMove = new PossibleMove { Position = tablesNotEmpty.First(x => x.Item.Content.Contains(IngredientName.ChoppedStrawberries)).Position, Name = IngredientName.ChoppedStrawberries };
                            possibleMove.CalculateScoreFromPosition(Game.MyChef.Position);
                            possibleMoves.Add(possibleMove);
                            break;
                    }

                    return possibleMoves;
                }
            }
        }

        public class PossibleMove
        {
            public string Name { get; set; }
            public Position Position { get; set; }
            public int Score { get; set; }

            public void CalculateScoreFromPosition(Position position)
            {
                Score = position.Manhattan(Position);
            }
        }

        public class Recipe
        {
            public string Name { get; set; }

            public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            public static Recipe DishIceCream = new Recipe
            {
                Name = $"{IngredientName.Dish}-{IngredientName.IceCream}",
                Ingredients = new List<Ingredient>
                {
                    Ingredient.Dish,
                    Ingredient.IceCream
                }
            };

            public static Recipe DishBlueberries = new Recipe
            {
                Name = $"{IngredientName.Dish}-{IngredientName.Blueberries}",
                Ingredients = new List<Ingredient>
                {
                    Ingredient.Dish,
                    Ingredient.Blueberries
                }
            };

            public static Recipe DishChoppedStrawberries = new Recipe
            {
                Name = $"{IngredientName.Dish}-{IngredientName.ChoppedStrawberries}",
                Ingredients = new List<Ingredient>
                {
                    Ingredient.Dish,
                    Ingredient.ChoppedStrawberries
                }
            };

            public PossibleMove GetBestMove(Game game, MainClass.GameTurn gameTurn)
            {
                var tablesNotEmpty = game.Tables.Where(x => x.Item != null).ToList();

                var readyDish = tablesNotEmpty.FirstOrDefault(x => x.Item.Content == Name);
                if (readyDish != null)
                    return new PossibleMove{ Name = Name, Position = readyDish.Position, Score = 0};

                var possibleMoves = GetPossibleMoves(game, gameTurn);
                return possibleMoves.OrderBy(x => x.Score).FirstOrDefault();
            }

            private List<PossibleMove> GetPossibleMoves(Game game, MainClass.GameTurn gameTurn)
            {
                return Ingredients.Select(x => x.GetBestMoveForIngredient(x.Name, game, gameTurn)).ToList();
            }
        }

        public class RecipeIngredient
        {
            public Recipe Ingredient { get; set; }
            public int Order { get; set; }
        }

        public class Ingredient
        {
            public string Name { get; set; }

            public PossibleMove GetBestMoveForIngredient(string ingredient, Game game, MainClass.GameTurn gameTurn)
            {
                return GetPossibleMovesForIngredient(ingredient, game, gameTurn).OrderBy(x => x.Score).FirstOrDefault();
            }

            private List<PossibleMove> GetPossibleMovesForIngredient(string ingredient, Game game, MainClass.GameTurn gameTurn)
            {
                var possibleMoves = new List<PossibleMove>();
                PossibleMove possibleMove;

                var tablesNotEmpty = game.Tables.Where(x => x.Item != null).ToList();

                switch (ingredient)
                {
                    case IngredientName.IceCream:
                        possibleMove = new PossibleMove { Position = game.IceCream.Position, Name = IngredientName.IceCream };
                        possibleMove.CalculateScoreFromPosition(game.MyChef.Position);
                        possibleMoves.Add(possibleMove);
                        break;
                    case IngredientName.Blueberries:
                        possibleMove = new PossibleMove { Position = game.Blueberry.Position, Name = IngredientName.Blueberries };
                        possibleMove.CalculateScoreFromPosition(game.MyChef.Position);
                        possibleMoves.Add(possibleMove);
                        break;
                    case IngredientName.ChoppedStrawberries:
                        possibleMove = new PossibleMove { Position = tablesNotEmpty.First(x => x.Item.Content.Contains(IngredientName.ChoppedStrawberries)).Position, Name = IngredientName.ChoppedStrawberries };
                        possibleMove.CalculateScoreFromPosition(game.MyChef.Position);
                        possibleMoves.Add(possibleMove);
                        break;
                }

                return possibleMoves;
            }

            public static Ingredient Dish = new Ingredient
            {
                Name = IngredientName.Dish
            };

            public static Ingredient IceCream = new Ingredient
            {
                Name = IngredientName.IceCream
            };

            public static Ingredient Blueberries = new Ingredient
            {
                Name = IngredientName.Blueberries
            };

            public static Ingredient ChoppedStrawberries = new Ingredient
            {
                Name = IngredientName.ChoppedStrawberries
            };
        }

        public class RecipeAction
        {
            public PossibleMove PossibleMove { get; set; }
            public int Order { get; set; }
        }

        public class IngredientName
        {
            public const string Dish = "DISH";
            public const string IceCream = "ICE_CREAM";
            public const string Blueberries = "BLUEBERRIES";
            public const string Strawberries = "STRAWBERRIES";
            public const string ChoppedStrawberries = "CHOPPED_STRAWBERRIES";
        }
    }
}
