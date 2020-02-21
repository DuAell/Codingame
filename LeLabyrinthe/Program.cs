using System;
using System.Linq;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Player
{


    static void Main(string[] args)
    {
        var game = new Game();
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        var R = int.Parse(inputs[0]); // number of rows.
        var C = int.Parse(inputs[1]); // number of columns.
        var A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int KR = int.Parse(inputs[0]); // row where Kirk is located.
            int KC = int.Parse(inputs[1]); // column where Kirk is located.
            game.Kirk.Position = new Position(KC, KR);
            for (int x = 0; x < C; x++)
            {
                string row = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                for (var y = 0; y < R; y++)
                {
                    var tile = Tile.Create(row[y], x, y);
                    game.Map.Add(tile);

                    if (tile is Start start)
                        game.Start = start;
                    if (tile is CommandRoom commandRoom)
                        game.CommandRoom = commandRoom;
                }
            }

            if (game.Kirk.Position.XY == game.CommandRoom?.Position.XY)
                game.HasReachedCommandRoom = true;

            var astarMap = new List<AStarSearch.Location>(game.Map.Select(x => new AStarSearch.Location(x.Position, x is Wall)));

            Console.Error.WriteLine(astarMap.Where(x => x.IsWall).Count());

            var astar = new AStarSearch(astarMap);
            var kirkLocation = astarMap.Find(_ => _.Position.XY == game.Kirk.Position.XY);

            if (!game.HasReachedCommandRoom && game.CommandRoom != null)
            {
                Console.Error.WriteLine("Heading towards command room");
                Console.WriteLine(game.Kirk.GetDirectionTo(game.CommandRoom.Position));
                continue;
            }
            if (game.HasReachedCommandRoom)
            {
                Console.Error.WriteLine("Escaping");
                var startLocation = astarMap.Find(_ => _.Position.XY == game.Start.Position.XY);
                var escape = astar.Compute(kirkLocation, startLocation);
                Console.WriteLine(game.Kirk.GetDirectionTo(escape.First().Position));
                //Console.WriteLine(game.Kirk.GetDirectionTo(game.Start.Position));
                continue;
            }

            Console.Error.WriteLine("Searching the command room");
            var nearestUnknownTiles = game.Map.OfType<Unknown>().OrderBy(x => game.Kirk.Position.Manhattan(x.Position));
            foreach (var nearestUnknownTile in nearestUnknownTiles)
            {
                var nearestUnknownTileLocation = astarMap.First(_ => _.Position.XY == nearestUnknownTile.Position.XY);
                var result = astar.Compute(kirkLocation, nearestUnknownTileLocation);
                if (result == null)
                    continue;
                Console.WriteLine(game.Kirk.GetDirectionTo(result.First().Position));
                break;
            }

            //var nearestUnknownTile = game.Map.OfType<Unknown>().OrderBy(x => game.Kirk.Position.Manhattan(x.Position)).First();
            //Console.WriteLine(game.Kirk.GetDirectionTo(nearestUnknownTile.Position));
        }
    }
}

public class Game
{
    public List<Tile> Map { get; set; } = new List<Tile>();
    public Kirk Kirk { get; } = new Kirk();
    public CommandRoom CommandRoom { get; set; }

    public Start Start { get; set; }

    public bool HasReachedCommandRoom { get; set; }
}

public abstract class Tile
{
    public Position Position { get; set; }

    public string GetDirectionTo(Position destination)
    {
        Console.Error.WriteLine($"Calculating destination from {Position.XY} to {destination.XY}");

        if (Position.X < destination.X)
            return "RIGHT";
        if (Position.X > destination.X)
            return "LEFT";
        if (Position.Y < destination.Y)
            return "DOWN";
        if (Position.Y > destination.Y)
            return "UP";

        return null;
    }

    public static Tile Create(char letter, int x, int y)
    {
        var position = new Position(x, y);

        switch (letter)
        {
            case '#':
                return new Wall()
                {
                    Position = position
                };
            case '.':
                return new Empty()
                {
                    Position = position
                };
            case 'T':
                return new Start()
                {
                    Position = position
                };
            case 'C':
                return new CommandRoom()
                {
                    Position = position
                };
            case '?':
                return new Unknown()
                {
                    Position = position
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(letter));
        }
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

    public int Manhattan(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);
}

public class Kirk : Tile
{

}

public class Unknown : Tile
{
}

public class Wall : Tile
{
}

public class Empty : Tile
{
}

public class CommandRoom : Tile
{
}

public class Start : Tile
{
}



public class AStarSearch
{
    public List<Location> Map { get; }

    public AStarSearch(List<Location> map)
    {
        Map = map;
    }

    public IEnumerable<Location> Compute(Location start, Location end)
    {
        Console.Error.WriteLine($"Starting to calculate path from {start.Position.XY} to {end.Position.XY}");
        var openList = new List<Location>();
        var closedList = new List<Location>();
        var hasFoundPath = false;
        var movementCost = 0;

        // start by adding the original position to the open list
        openList.Add(start);

        while (openList.Count > 0)
        {
            // get the square with the lowest F score
            var lowest = openList.Min(l => l.TotalScore);
            var current = openList.First(l => l.TotalScore == lowest);

            Console.Error.WriteLine($"Trying {current.Position.XY}");

            // add the current square to the closed list
            closedList.Add(current);

            // remove it from the open list
            openList.Remove(current);

            // if we added the destination to the closed list, we've found a path
            if (current.Position.XY == end.Position.XY)
            {
                Console.Error.WriteLine($"Found a way to {end.Position.XY}");
                hasFoundPath = true;
                break;
            }

            var adjacentSquares = GetWalkableAdjacentSquares(current, Map);
            movementCost++;

            foreach (var adjacentSquare in adjacentSquares)
            {
                // if this adjacent square is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.Position.X == adjacentSquare.Position.X
                                                   && l.Position.Y == adjacentSquare.Position.Y) != null)
                    continue;

                // if it's not in the open list...
                if (openList.FirstOrDefault(l => l.Position.X == adjacentSquare.Position.X
                                                 && l.Position.Y == adjacentSquare.Position.Y) == null)
                {
                    // compute its score, set the parent
                    adjacentSquare.CostFromStart = movementCost;
                    adjacentSquare.CostToDestination = adjacentSquare.Position.Manhattan(end.Position);
                    adjacentSquare.Parent = current;

                    Console.Error.WriteLine($"Adding {current.Position.XY} as parent of {adjacentSquare.Position.XY}");

                    // and add it to the open list
                    openList.Insert(0, adjacentSquare);
                }
                else
                {
                    // test if using the current score makes the adjacent square's F score lower, if yes update the parent because it means it's a better path
                    if (movementCost + adjacentSquare.CostToDestination < adjacentSquare.TotalScore)
                    {
                        adjacentSquare.CostFromStart = movementCost;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        }

        if (!hasFoundPath)
            return null;

        var result = new List<Location>();
        var currentResult = end;

        while (currentResult != null)
        {
            Console.Error.WriteLine($"Adding {currentResult.Position.XY} to list");
            result.Add(currentResult);
            currentResult = currentResult.Parent;
        }

        result.Reverse();
        result.Remove(start);

        Console.Error.WriteLine($"Calculated path: {string.Join("->", result.Select(x => x.Position.XY))}");

        return result;
    }

    private static IEnumerable<Location> GetWalkableAdjacentSquares(Location current, List<Location> map)
    {
        var proposedLocations = new List<Location>();

        AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == current.Position.X && _.Position.Y == current.Position.Y - 1), proposedLocations);
        AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == current.Position.X && _.Position.Y == current.Position.Y + 1), proposedLocations);
        AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == current.Position.X - 1 && _.Position.Y == current.Position.Y), proposedLocations);
        AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == current.Position.X + 1 && _.Position.Y == current.Position.Y), proposedLocations);

        return proposedLocations.Where(_ => !_.IsWall);
    }

    private static void AddIfNotNull<T>(T item, List<T> list)
    {
        if (item != null)
            list.Add(item);
    }

    public class Location
    {
        public Location(Position position, bool isWall)
        {
            Position = position;
            IsWall = isWall;
        }

        public Position Position { get; }
        public bool IsWall { get; }

        /// <summary>
        /// Score
        /// </summary>
        public int TotalScore => CostFromStart + CostToDestination;
        /// <summary>
        /// Movement cost from start to here
        /// </summary>
        public int CostFromStart { get; set; }
        /// <summary>
        /// Estimated movement cost from here to destination
        /// </summary>
        public int CostToDestination { get; set; }

        public Location Parent { get; set; }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Common;

//namespace MyDjikstra
//{
//    public class SearchEngine
//    {
//        public event EventHandler Updated;
//        private void OnUpdated()
//        {
//            Updated?.Invoke(null, EventArgs.Empty);
//        }
//        public Map Map { get; set; }
//        public Node Start { get; set; }
//        public Node End { get; set; }
//        public int NodeVisits { get; private set; }
//        public double ShortestPathLength { get; set; }
//        public double ShortestPathCost { get; private set; }

//        public SearchEngine(Map map)
//        {
//            Map = map;
//            End = map.EndNode;
//            Start = map.StartNode;
//        }

//        private void BuildShortestPath(List<Node> list, Node node)
//        {
//            if (node.NearestToStart == null)
//                return;
//            list.Add(node.NearestToStart);
//            ShortestPathLength += node.Connections.Single(x => x.ConnectedNode == node.NearestToStart).Length;
//            ShortestPathCost += node.Connections.Single(x => x.ConnectedNode == node.NearestToStart).Cost;
//            BuildShortestPath(list, node.NearestToStart);
//        }

//        public List<Node> GetShortestPathAstart()
//        {
//            foreach (var node in Map.Nodes)
//                node.StraightLineDistanceToEnd = node.StraightLineDistanceTo(End);
//            AstarSearch();
//            var shortestPath = new List<Node>();
//            shortestPath.Add(End);
//            BuildShortestPath(shortestPath, End);
//            shortestPath.Reverse();
//            return shortestPath;
//        }

//        private void AstarSearch()
//        {
//            NodeVisits = 0;
//            Start.MinCostToStart = 0;
//            var prioQueue = new List<Node>();
//            prioQueue.Add(Start);
//            do
//            {
//                prioQueue = prioQueue.OrderBy(x => x.MinCostToStart + x.StraightLineDistanceToEnd).ToList();
//                var node = prioQueue.First();
//                prioQueue.Remove(node);
//                NodeVisits++;
//                foreach (var cnn in node.Connections.OrderBy(x => x.Cost))
//                {
//                    var childNode = cnn.ConnectedNode;
//                    if (childNode.Visited)
//                        continue;
//                    if (childNode.MinCostToStart == null ||
//                        node.MinCostToStart + cnn.Cost < childNode.MinCostToStart)
//                    {
//                        childNode.MinCostToStart = node.MinCostToStart + cnn.Cost;
//                        childNode.NearestToStart = node;
//                        if (!prioQueue.Contains(childNode))
//                            prioQueue.Add(childNode);
//                    }
//                }
//                node.Visited = true;
//                if (node == End)
//                    return;
//            } while (prioQueue.Any());
//        }
//    }
//}