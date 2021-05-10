using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SpringChallenge2021
{
    internal class Cell
    {
        public int Index;
        private readonly int[] _neighboursIndex;
        public Neighbour[] Neighbours = new Neighbour[6];
        public int Richness;

        public Cell(int index, int richness, int[] neighboursIndex)
        {
            Index = index;
            Richness = richness;
            _neighboursIndex = neighboursIndex;
        }

        public void CalculateNeighbours(List<Cell> board)
        {
            for (var i = 0; i <= 5; i++)
            {
                if (_neighboursIndex[i] == -1)
                    continue;
                Neighbours[i] = new Neighbour(board.Single(_ => _.Index == _neighboursIndex[i]), i);
            }
        }

        public IReadOnlyCollection<Tree> GetShadowingTrees(List<Tree> trees, int sunDirection)
        {
            var result = new List<Tree>();
            var oppositeDirection = (sunDirection < 3) ? (sunDirection + 3) : (sunDirection - 3);
            var cell = this;
            for (var i = 1; i <= 3; i++)
            {
                var neighbour = cell.Neighbours[oppositeDirection]?.Cell;
                if (neighbour == null)
                    continue;
                var neighbourTree = trees.SingleOrDefault(_ => _.Cell == neighbour);
                if (neighbourTree != null && neighbourTree.Size >= i)
                {
                    result.Add(neighbourTree);
                }

                cell = neighbour;
            }

            return result;
        }
    }

    internal class Neighbour
    {
        public Cell Cell { get; }
        public int Direction { get; }

        public Neighbour(Cell cell, int direction)
        {
            Cell = cell;
            Direction = direction;
        }
    }

    internal class Tree
    {
        public Cell Cell;
        public bool IsDormant;
        public bool IsMine;
        public int Size;

        public Tree(Cell cell, int size, bool isMine, bool isDormant)
        {
            Cell = cell;
            Size = size;
            IsMine = isMine;
            IsDormant = isDormant;
        }

        public IReadOnlyCollection<Tree> GetThreateningTrees(List<Tree> trees, int sunDirection, int? overrideSize = null)
        {
            var shadowingTrees = Cell.GetShadowingTrees(trees, sunDirection);

            return shadowingTrees.Where(_ => _.Size >= (overrideSize ?? Size)).ToList();
        }

        public IReadOnlyCollection<Tree> GetThreatenedTrees(List<Tree> trees, int sunDirection, int? overrideSize = null)
        {
            var size = overrideSize ?? Size;
            var result = new List<Tree>();
            var cell = Cell;
            for (var i = 1; i <= size; i++)
            {
                var neighbour = cell.Neighbours[sunDirection]?.Cell;
                if (neighbour == null)
                    continue;
                var neighbourTree = trees.SingleOrDefault(_ => _.Cell == neighbour);
                if (neighbourTree != null && neighbourTree.Size <= size)
                {
                    result.Add(neighbourTree);
                }

                cell = neighbour;
            }

            return result;
        }

        public int TimeToGrow() => 3 - Size;
    }

    internal class Action
    {
        public const string Wait = "WAIT";
        public const string Seed = "SEED";
        public const string Grow = "GROW";
        public const string Complete = "COMPLETE";
        public int SourceCellIdx;
        public int TargetCellIdx;

        public List<Score> Scores { get; set; } = new List<Score>();

        public static Action Parse(string action, Game game)
        {
            string[] parts = action.Split(" ");
            switch (parts[0])
            {
                case Wait:
                    return new ActionWait();
                case Seed:
                    return new ActionSeed(game.Trees.Single(_ => _.Cell.Index == int.Parse(parts[1])), game.Board.Single(_ => _.Index == int.Parse(parts[2])));
                case Grow:
                    return new ActionGrow(game.Trees.Single(_ => _.Cell.Index == int.Parse(parts[1])));
                case Complete:
                    return new ActionComplete(game.Trees.Single(_ => _.Cell.Index == int.Parse(parts[1])));
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual int GetCost(Game game)
        {
            return 0;
        }

        public int GetScore(Game game)
        {
            return Scores.Sum(_ => _.Total);
        }

        public string Type;

        public Action(string type, int sourceCellIdx, int targetCellIdx)
        {
            Type = type;
            TargetCellIdx = targetCellIdx;
            SourceCellIdx = sourceCellIdx;
        }

        public Action(string type, int targetCellIdx)
            : this(type, 0, targetCellIdx)
        {
        }

        public Action(string type)
            : this(type, 0, 0)
        {
        }

        public virtual void ComputeScore(Game game)
        {
            
        }

        public override string ToString()
        {
            if (Type == Wait) return Wait;
            if (Type == Seed) return $"{Seed} {SourceCellIdx} {TargetCellIdx}";
            return $"{Type} {TargetCellIdx}";
        }
    }

    internal class ActionWait : Action
    {
        public ActionWait() : base(Wait)
        {

        }
    }

    internal class ActionComplete : Action
    {
        public Tree Tree { get; }

        public ActionComplete(Tree tree) : base(Complete, tree.Cell.Index)
        {
            Tree = tree;
        }

        public override int GetCost(Game game)
        {
            return 4;
        }

        public override void ComputeScore(Game game)
        {
            var tomorrowSunDirection = Game.SunDirection(game.Day + 1);

            Tree.GetThreatenedTrees(game.Trees, tomorrowSunDirection, Tree.Size + 1).Where(_ => _.IsMine && _.Size > 0).ToList().ForEach(_ => Scores.Add(new Score(_.Size, $"Ting my:{_.Cell.Index}")));
            Tree.GetThreatenedTrees(game.Trees, tomorrowSunDirection, Tree.Size + 1).Where(_ => !_.IsMine && _.Size > 0).ToList().ForEach(_ => Scores.Add(new Score(-_.Size, $"Ting op:{_.Cell.Index}")));
            Scores.Add(new Score(Tree.Cell.Richness, $"Richness"));
            Scores.Add(new Score(2, $"Priority")); // Better to grow nearly complete trees
        }
    }

    internal class ActionGrow : Action
    {
        public Tree Tree { get; }

        public ActionGrow(Tree tree) : base(Grow, tree.Cell.Index)
        {
            Tree = tree;
        }

        public bool HasTimeToGrow(Game game)
        {
            return Tree.TimeToGrow() < game.NumberOfDaysLeft();
        }

        public override int GetCost(Game game)
        {
            // Faire pousser une graine en un arbre de taille 1 coûte 1 point de soleil + le nombre d'arbres de taille 1 que vous possédez déjà.
            if (Tree.Size == 0)
            {
                return game.Trees.Count(_ => _.IsMine && _.Size == 1) + 1;
            }

            //Faire pousser un arbre de taille 1 en un arbre de taille 2 coûte 3 points de soleil + le nombre d'arbres de taille 2 que vous possédez déjà.
            if (Tree.Size == 1)
            {
                return game.Trees.Count(_ => _.IsMine && _.Size == 2) + 3;
            }

            //Faire pousser un arbre de taille 2 en un arbre de taille 3 coûte 7 points de soleil + le nombre d'arbres de taille 3 que vous possédez déjà.
            if (Tree.Size == 2)
            {
                return game.Trees.Count(_ => _.IsMine && _.Size == 3) + 7;
            }

            throw new NotImplementedException();
        }

        public override void ComputeScore(Game game)
        {
            var tomorrowSunDirection = Game.SunDirection(game.Day + 1);

            if (!HasTimeToGrow(game))
                Scores.Add(new Score(-10, "No time"));
            Tree.GetThreateningTrees(game.Trees, tomorrowSunDirection, Tree.Size + 1).ToList().ForEach(_ => Scores.Add(new Score(-5, $"Td by {_.Cell.Index}"))); // Remove points if tree will be threatened tomorrow
            Tree.GetThreatenedTrees(game.Trees, tomorrowSunDirection, Tree.Size + 1).Where(_ => _.IsMine && _.Size > 0).ToList().ForEach(_ => Scores.Add(new Score(-_.Size, $"Ting my:{_.Cell.Index}")));
            Tree.GetThreatenedTrees(game.Trees, tomorrowSunDirection, Tree.Size + 1).Where(_ => !_.IsMine && _.Size > 0).ToList().ForEach(_ => Scores.Add(new Score(_.Size, $"Ting op:{_.Cell.Index}")));
            Scores.Add(new Score(Tree.Cell.Richness, $"Richness"));
            Scores.Add(new Score(Tree.Size, $"Size")); // Better to grow nearly complete trees
        }
    }

    internal class ActionSeed : Action
    {
        public Tree Tree { get; }
        public Cell Target { get; }

        public ActionSeed(Tree tree, Cell target) : base(Seed, tree.Cell.Index, target.Index)
        {
            Tree = tree;
            Target = target;
        }

        public override int GetCost(Game game)
        {
            //Pour effectuer l'action Seed, vous devez dépenser un nombre de points de soleil égal au nombre de graines (arbres de taille 0) présentes dans la forêt que vous possédez.
            return game.Trees.Count(_ => _.IsMine && _.Size == 0);
        }

        public override void ComputeScore(Game game)
        {
            if (!HasTimeToGrow(game)) 
                Scores.Add(new Score(-10, "No time"));

            Scores.Add(new Score(Tree.Cell.Richness, "Richness"));
        }

        public bool HasTimeToGrow(Game game)
        {
            return 3 <= game.NumberOfDaysLeft();
        }
    }

    internal class Score
    {
        public int Total { get; }
        public string Comment { get; }

        public Score(int total, string comment = null)
        {
            Total = total;
            Comment = comment;
        }
    }

    internal class Game
    {
        public const int MaxDays = 23; // Base 0 (6 days => 5)

        public List<Cell> Board;
        public int Day; // Base 0
        public int MyScore, OpponentScore;
        public int MySun, OpponentSun;
        public int Nutrients;
        public bool OpponentIsWaiting;
        public List<Action> PossibleActions;
        public List<Action> PossibleMoves;
        public List<Tree> Trees;

        public Game()
        {
            Board = new List<Cell>();
            PossibleActions = new List<Action>();
            Trees = new List<Tree>();
        }

        public int NumberOfDaysLeft() => MaxDays - Day;

        public Action GetNextAction()
        {
            PossibleActions.AddRange(PossibleMoves);

            PossibleActions.ForEach(_ => _.ComputeScore(this));

            var timeToGrowAllTrees = Trees.Where(_ => _.IsMine).Sum(_ => _.TimeToGrow());
            if (NumberOfDaysLeft() > timeToGrowAllTrees) // Stop seeding if we don't have time to grow them all
                PossibleActions.RemoveAll(_ => _ is ActionSeed);


            return PossibleMoves.OrderByDescending(_ => _.GetScore(this)).First();
        }

        public static int SunDirection(int day) => day % 6;
    }

    internal class Player
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        // ReSharper disable once UnusedMember.Local
        private static void Main()
        {
            string[] inputs;

            var game = new Game();

            var numberOfCells = int.Parse(Console.ReadLine()); // 37
            for (var i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
                var richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
                var neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
                var neigh1 = int.Parse(inputs[3]);
                var neigh2 = int.Parse(inputs[4]);
                var neigh3 = int.Parse(inputs[5]);
                var neigh4 = int.Parse(inputs[6]);
                var neigh5 = int.Parse(inputs[7]);
                int[] neighs = {neigh0, neigh1, neigh2, neigh3, neigh4, neigh5};
                var cell = new Cell(index, richness, neighs);
                game.Board.Add(cell);
            }
            game.Board.ForEach(_ => _.CalculateNeighbours(game.Board));

            // game loop
            while (true)
            {
                game.Day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
                game.Nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
                inputs = Console.ReadLine().Split(' ');
                game.MySun = int.Parse(inputs[0]); // your sun points
                game.MyScore = int.Parse(inputs[1]); // your current score
                inputs = Console.ReadLine().Split(' ');
                game.OpponentSun = int.Parse(inputs[0]); // opponent's sun points
                game.OpponentScore = int.Parse(inputs[1]); // opponent's score
                game.OpponentIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day
                Console.Error.WriteLine($"{game.NumberOfDaysLeft()} days left");
                Console.Error.WriteLine($"Sun direction: {Game.SunDirection(game.Day)}");

                game.Trees.Clear();
                var numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
                for (var i = 0; i < numberOfTrees; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var cellIndex = int.Parse(inputs[0]); // location of this tree
                    var size = int.Parse(inputs[1]); // size of this tree: 0-3
                    var isMine = inputs[2] != "0"; // 1 if this is your tree
                    var isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                    var tree = new Tree(game.Board.Single(_ => _.Index == cellIndex), size, isMine, isDormant);
                    game.Trees.Add(tree);
                }

                //foreach (var cell in game.Board)
                //{
                //    var shadowingTrees = cell.GetShadowingTrees(game.Trees, Game.SunDirection(game.Day));
                //    if (!shadowingTrees.Any())
                //        continue;
                //    Console.Error.WriteLine($"Cell {cell.Index} is shadowed by tree(s) {string.Join(",", shadowingTrees.Select(_ => _.Cell.Index))}");
                //}

                //foreach (var tree in game.Trees)
                //{
                //    var threateningTrees = tree.GetThreateningTrees(game.Trees, Game.SunDirection(game.Day));
                //    if (!threateningTrees.Any())
                //        continue;
                //    Console.Error.WriteLine($"Tree {tree.Cell.Index} is threatened by tree(s) {string.Join(",", threateningTrees.Select(_ => _.Cell.Index))}");
                //}

                game.PossibleActions.Clear();
                var numberOfPossibleMoves = int.Parse(Console.ReadLine());
                game.PossibleMoves = new List<Action>();
                for (var i = 0; i < numberOfPossibleMoves; i++)
                {
                    var possibleMove = Console.ReadLine();
                    game.PossibleMoves.Add(Action.Parse(possibleMove, game));
                }

                var action = game.GetNextAction();

                game.PossibleActions.OrderByDescending(_ => _.GetScore(game)).ToList()
                    .ForEach(_ => Console.Error.WriteLine($"{_}: {_.GetScore(game)} ({string.Join(",", _.Scores.Select(s => $"{s.Comment} ({s.Total})"))})"));

                Console.WriteLine(action);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}