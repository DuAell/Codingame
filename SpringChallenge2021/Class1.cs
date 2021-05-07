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
        public Cell[] Neighbours;
        public int Richness;

        public Cell(int index, int richness, int[] neighboursIndex)
        {
            Index = index;
            Richness = richness;
            _neighboursIndex = neighboursIndex;
        }

        public void CalculateNeighbours(List<Cell> board)
        {
            Neighbours = board.Where(_ => _neighboursIndex.Contains(_.Index)).ToArray();
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

        public bool HasTimeToGrow(Game game)
        {
            return Size - game.NumberOfDaysLeft() <= 0;
        }
    }

    internal class Action
    {
        public const string Wait = "WAIT";
        public const string Seed = "SEED";
        public const string Grow = "GROW";
        public const string Complete = "COMPLETE";
        public int SourceCellIdx;
        public int TargetCellIdx;

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
    }

    internal class ActionGrow : Action
    {
        public Tree Tree { get; }

        public ActionGrow(Tree tree) : base(Grow, tree.Cell.Index)
        {
            Tree = tree;
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
        public List<Tree> Trees;

        public Game()
        {
            Board = new List<Cell>();
            PossibleActions = new List<Action>();
            Trees = new List<Tree>();
        }

        public int NumberOfDaysLeft()
        {
            return MaxDays - Day;
        }

        public Action GetNextAction(List<Action> possibleMoves)
        {
            PossibleActions.AddRange(possibleMoves.OfType<ActionComplete>().OrderByDescending(_ => _.Tree.Cell.Richness));

            PossibleActions.AddRange(possibleMoves.OfType<ActionGrow>().Where(_ => _.Tree.HasTimeToGrow(this)).OrderByDescending(_ => _.Tree.Cell.Richness).ThenByDescending(_ => _.Tree.Size));

            PossibleActions.AddRange(possibleMoves.OfType<ActionSeed>().OrderByDescending(_ => _.Target.Richness));

            PossibleActions.AddRange(possibleMoves.OfType<ActionWait>());

            PossibleActions.RemoveAll(_ => !possibleMoves.Contains(_));

            PossibleActions.ForEach(_ => Console.Error.WriteLine($"Possible action: {_}"));

            return PossibleActions.First();
        }
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

                game.PossibleActions.Clear();
                var numberOfPossibleMoves = int.Parse(Console.ReadLine());
                var possibleMoves = new List<Action>();
                for (var i = 0; i < numberOfPossibleMoves; i++)
                {
                    var possibleMove = Console.ReadLine();
                    possibleMoves.Add(Action.Parse(possibleMove, game));
                    Console.Error.WriteLine(possibleMove);
                }

                var action = game.GetNextAction(possibleMoves);
                Console.WriteLine(action);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}