using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        game.TurnsBeforeAlarm = A;

        AStarSearch.Route routeToFollow = null;

        // game loop
        while (true)
        {
            game.MovesLeft--;
            Console.Error.WriteLine($"Moves left: {game.MovesLeft}");

            inputs = Console.ReadLine().Split(' ');
            int KR = int.Parse(inputs[0]); // row where Kirk is located.
            int KC = int.Parse(inputs[1]); // column where Kirk is located.
            game.Kirk.Position = new Position(KC, KR);
            Console.Error.WriteLine($"Kirk position: {game.Kirk.Position.XY}");

            game.Map = new List<Tile>();

            for (int y = 0; y < R; y++)
            {
                string row = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                for (var x = 0; x < C; x++)
                {
                    var tile = Tile.Create(row[x], x, y);
                    game.Map.Add(tile);

                    if (tile is Start start)
                        game.Start = start;
                    if (tile is CommandRoom commandRoom)
                        game.CommandRoom = commandRoom;
                }
            }

            Console.Error.WriteLine($"Start position: {game.Start.Position.XY}");
            Console.Error.WriteLine($"Command room position: {game.CommandRoom?.Position.XY}");

            if (routeToFollow?.IsValid == true)
            {
                Console.Error.WriteLine($"Following route {routeToFollow}");
                WriteNextMove(game, routeToFollow);
                continue;
            }
            
            if (game.Kirk.Position.XY == game.CommandRoom?.Position.XY)
                game.HasReachedCommandRoom = true;

            List<AStarSearch.Location> astarMap;

            if (game.CommandRoom != null)
            {
                var knownMap = new List<AStarSearch.Location>(game.Map.Select(x =>
                    new AStarSearch.Location(x.Position, x is Wall || x is Unknown)));

                var astar = new AStarSearch(knownMap);
                var kirkLocation = knownMap.Find(_ => _.Position.XY == game.Kirk.Position.XY);
                
                //var startRoute = astar.Compute(kirkLocation, startLocation);
                //Console.Error.WriteLine($"Start route: {startRoute}");

                //if (game.HasReachedCommandRoom && startRoute.Distance <= game.TurnsBeforeAlarm)
                //{
                //    Console.Error.WriteLine($"Alarm in {game.TurnsBeforeAlarm} turns");
                //    game.TurnsBeforeAlarm--;
                //    Console.Error.WriteLine($"Escaping. Distance: {startRoute.Distance}");
                //    routeToFollow = startRoute;
                //    WriteNextMove(game, routeToFollow);
                //    continue;
                //}

                var commandRoomLocation = knownMap.Find(_ => _.Position.XY == game.CommandRoom.Position.XY);
                var commandRoomRoute = astar.Compute(kirkLocation, commandRoomLocation); // Can be invalid if we have detected it through a wall but don't know how to get to it

                if (commandRoomRoute.IsValid)
                {
                    var escapeMap = new List<AStarSearch.Location>(game.Map.Select(x =>
                        new AStarSearch.Location(x.Position, x is Wall || x is Unknown)));
                    var escapeAStar = new AStarSearch(escapeMap);
                    var startLocation = escapeMap.Find(_ => _.Position.XY == game.Start.Position.XY);
                    commandRoomLocation = escapeMap.Find(_ => _.Position.XY == game.CommandRoom.Position.XY);
                    var commandRoomToStartRoute = escapeAStar.Compute(commandRoomLocation, startLocation);
                    var fullRoute = new AStarSearch.Route
                    {
                        Positions = commandRoomRoute.Positions
                    };
                    fullRoute.Positions.AddRange(commandRoomToStartRoute.Positions);

                    // With a known route, Kirk can go to command room and escape. So let's do it.
                    if (fullRoute.IsValid && commandRoomToStartRoute.Distance <= game.TurnsBeforeAlarm)
                    {
                        Console.Error.WriteLine($"Full route found. Turns to go and escape : {fullRoute.Distance}. Route: {fullRoute}");
                        routeToFollow = fullRoute;
                        WriteNextMove(game, routeToFollow);
                        continue;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Not enough time to escape. Need {fullRoute.Distance} and have only {game.TurnsBeforeAlarm}. Route: {fullRoute}");
                    }
                }

                //// With a known route, Kick can go to command room and escape. So let's do it.
                //if (commandRoomRoute.IsValid && commandRoomToStartRoute.IsValid && commandRoomRoute.Distance + commandRoomToStartRoute.Distance <= game.TurnsBeforeAlarm)
                //{
                //    Console.Error.WriteLine($"Going to command room. Turns to go and escape : {commandRoomRoute.Distance + commandRoomToStartRoute.Distance}");
                //    Console.Error.WriteLine($"Command room route: {commandRoomRoute}");
                //    routeToFollow = commandRoomRoute;
                //    WriteNextMove(game, routeToFollow);
                //    continue;
                //}
                //else // We do not have any known route that allows to go to command room and escape in time. Keep exploring !
                //{
                //    if (commandRoomRoute.IsValid)
                //        Console.Error.WriteLine($"Not enough time to escape. Need {startRoute.Distance + commandRoomRoute.Distance} and have only {game.TurnsBeforeAlarm}");
                    Console.Error.WriteLine("Continue exploration");
                    var heuristicMap = new List<AStarSearch.Location>(game.Map.Select(x => new AStarSearch.Location(x.Position, x is Wall)));
                    heuristicMap.Find(_ => _.Position.XY == game.CommandRoom.Position.XY).IsWall = true; // Consider command room as a wall to exclude it

                    ExploreUnknown(game, heuristicMap);
                    continue;
                //}
            }

            Console.Error.WriteLine("Searching the command room");
            astarMap = new List<AStarSearch.Location>(game.Map.Select(x => new AStarSearch.Location(x.Position, x is Wall)));
            ExploreUnknown(game, astarMap);
            continue;
        }
    }

    private static void WriteNextMove(Game game, AStarSearch.Route route)
    {
        var nextMove = route.Positions.First();
        route.Positions.Remove(nextMove);
        Console.WriteLine(game.Kirk.GetDirectionTo(nextMove));
    }

    static void ExploreUnknown(Game game, List<AStarSearch.Location> astarMap)
    {
        var kirkLocation = astarMap.Find(_ => _.Position.XY == game.Kirk.Position.XY);
        var astar = new AStarSearch(astarMap);

        var unknownMap = game.Map.Where(x => x.GetType() == typeof(Unknown)).ToList();
        var nearestUnknownTiles = unknownMap.OrderBy(x => game.Kirk.Position.Manhattan(x.Position)).ThenByDescending(x => x.GetAdjacentSquares(unknownMap).Count());
        foreach (var nearestUnknownTile in nearestUnknownTiles)
        {
            var nearestUnknownTileLocation = astarMap.First(_ => _.Position.XY == nearestUnknownTile.Position.XY);
            var result = astar.Compute(kirkLocation, nearestUnknownTileLocation);
            if (!result.IsValid)
                continue;
            Console.Error.WriteLine($"Next unknown tile route: {result}");
            Console.WriteLine(game.Kirk.GetDirectionTo(result.Positions.First()));
            break;
        }
    }
}

public static class Helper
{
    public static void AddIfNotNull<T>(T item, List<T> list)
    {
        if (item != null)
            list.Add(item);
    }
}

public class Game
{
    public List<Tile> Map { get; set; } = new List<Tile>();
    public Kirk Kirk { get; } = new Kirk();
    public CommandRoom CommandRoom { get; set; }
    public int MovesLeft { get; set; } = 1200;

    public Start Start { get; set; }

    public bool HasReachedCommandRoom { get; set; }
    public int TurnsBeforeAlarm { get; set; }
}

public abstract class Tile
{
    public Position Position { get; set; }

    public string GetDirectionTo(Position destination)
    {
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

    public IEnumerable<Tile> GetAdjacentSquares(List<Tile> map)
    {
        var proposedLocations = new List<Tile>();

        Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y - 1), proposedLocations);
        Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y + 1), proposedLocations);
        Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X - 1 && _.Position.Y == Position.Y), proposedLocations);
        Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X + 1 && _.Position.Y == Position.Y), proposedLocations);

        return proposedLocations;
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

    public IEnumerable<Position> GetAdjacent(List<Position> map)
    {
        yield return map.FirstOrDefault(_ => _.X == X && _.Y == Y - 1);
        yield return map.FirstOrDefault(_ => _.X == X && _.Y == Y + 1);
        yield return map.FirstOrDefault(_ => _.X == X - 1 && _.Y == Y);
        yield return map.FirstOrDefault(_ => _.X == X + 1 && _.Y == Y);
    }
}

public class Kirk : Empty
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

public class CommandRoom : Empty
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

    public Route Compute(Location start, Location end)
    {
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

            // add the current square to the closed list
            closedList.Add(current);

            // remove it from the open list
            openList.Remove(current);

            // if we added the destination to the closed list, we've found a path
            if (current.Position.XY == end.Position.XY)
            {
                hasFoundPath = true;
                break;
            }

            var adjacentSquares = current.GetAdjacentSquares(Map).Where(_ => !_.IsWall).ToList();
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
            return new Route();

        var result = new List<Location>();
        var currentResult = end;

        while (currentResult != null)
        {
            result.Add(currentResult);
            currentResult = currentResult.Parent;
        }

        result.Reverse();
        result.Remove(start);

        var route = new Route
        {
            Positions = result.Select(x => x.Position).ToList()
        };

        return route;
    }

    public class Location
    {
        public Location(Position position, bool isWall)
        {
            Position = position;
            IsWall = isWall;
        }

        public Position Position { get; }
        public bool IsWall { get; set; }

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

        public IEnumerable<Location> GetAdjacentSquares(List<Location> map)
        {
            var proposedLocations = new List<Location>();

            Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y - 1), proposedLocations);
            Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y + 1), proposedLocations);
            Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X - 1 && _.Position.Y == Position.Y), proposedLocations);
            Helper.AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X + 1 && _.Position.Y == Position.Y), proposedLocations);

            return proposedLocations;
        }
    }

    public class Route
    {
        public List<Position> Positions { get; set; } = new List<Position>();

        public bool IsValid => Positions.Any();

        public int Distance => Positions.Count;

        public override string ToString()
        {
            return $"[Distance: {Distance}] {string.Join("->", Positions.Select(x => x.XY))}";
        }
    }
}