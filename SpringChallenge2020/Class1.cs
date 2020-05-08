using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace SpringChallenge2020
{
    /**
 * Grab the pellets as fast as you can!
 **/
    public class Player
    {
        public static void Main(string[] args)
        {
            Game.Play();
        }

        public static class Game
        {
            public static void Play(string initData = null, Queue<string> turnDatas = null)
            {
                var initInputProcessor = new InputProcessor(initData);
                var inputs = initInputProcessor.ReadLine().Split(' ');
                var map = new Map();
                int width = int.Parse(inputs[0]); // size of the grid
                int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
                for (int y = 0; y < height; y++)
                {
                    string row = initInputProcessor.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
                    for (var x = 0; x < width; x++)
                    {
                        map.Tiles.Add(new Tile(new Position(x, y), row[x] == '#'));
                    }
                    map.Tiles.Where(_ => _.IsWall).ToList().ForEach(_ => _.Unknown = false);
                }

                var pacs = new List<Pac>();

                // game loop
                while (true)
                {
                    pacs.ForEach(_ => _.IsAlive = false);
                    pacs.ForEach(_ => _.IsVisible = false);
                    if (turnDatas != null && turnDatas.Count == 0)
                        break;
                    var turnData = turnDatas?.Count > 0 ? turnDatas.Dequeue() : null;
                    var gameTurnInputProcessor = new InputProcessor(turnData);

                    inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                    int myScore = int.Parse(inputs[0]);
                    int opponentScore = int.Parse(inputs[1]);
                    int visiblePacCount = int.Parse(gameTurnInputProcessor.ReadLine()); // all your pacs and enemy pacs in sight
                    for (int i = 0; i < visiblePacCount; i++)
                    {
                        inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                        int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                        bool mine = inputs[1] != "0"; // true if this pac is yours
                        int x = int.Parse(inputs[2]); // position in the grid
                        int y = int.Parse(inputs[3]); // position in the grid
                        string typeId = inputs[4]; // unused in wood leagues
                        int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                        int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

                        if (!pacs.Any(_ => _.Id == pacId && _.IsMine == mine))
                            pacs.Add(new Pac { Id = pacId, IsMine = mine });
                        var pac = pacs.First(_ => _.Id == pacId && _.IsMine == mine);
                        pac.PreviousPosition = pac.Position;
                        pac.Position = new Position(x, y);
                        pac.PacType = Enum.Parse<PacType>(typeId);
                        pac.IsAlive = true;
                        pac.SpeedTurnsLeft = speedTurnsLeft;
                        pac.AbilityCooldown = abilityCooldown;
                        pac.IsVisible = true;
                    }

                    pacs.RemoveAll(_ => !_.IsAlive);

                    var visiblePelletCount = int.Parse(gameTurnInputProcessor.ReadLine()); // all pellets in sight
                    var visiblePellets = new List<Position>();

                    for (int i = 0; i < visiblePelletCount; i++)
                    {
                        inputs = gameTurnInputProcessor.ReadLine().Split(' ');
                        var x = int.Parse(inputs[0]);
                        var y = int.Parse(inputs[1]);

                        visiblePellets.Add(new Position(x, y));

                        var tile = map.GetTile(new Position(x, y));
                        tile.Unknown = false;
                        tile.Pellet = new Pellet {Value = int.Parse(inputs[2])};
                    }

                    initInputProcessor.WriteDebugData();
                    gameTurnInputProcessor.WriteDebugData();

                    // Remove eaten pellets
                    foreach (var pac in pacs)
                    {
                        var tile = map.GetTile(pac.Position);
                        tile.Pellet = null;
                    }

                    // Remove pellets when tile is in line of sight
                    foreach (var pac in pacs.Where(_ => _.IsMine))
                    {
                        var tiles = map.GetTilesInSightFromAllDirections(pac.Position);
                        foreach (var tile in tiles)
                        {
                            tile.Unknown = false;
                            if (visiblePellets.All(_ => _.XY != tile.Position.XY))
                                tile.Pellet = null; // This tile is not in the list of visible pellets but is in line of sight. So it's empty
                        }
                    }

                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");
                    foreach (var pac in pacs.Where(_ => _.IsMine))
                    {
                        pac.CommandArgs = string.Empty;
                        pac.CommandMessage = string.Empty;

                        var tilesInSight = map.GetTilesInSightFromAllDirections(pac.Position);

                        // Close to an opponent
                        var opponent = pacs.FirstOrDefault(_ => !_.IsMine && _.IsVisible && tilesInSight.Select(t => t.Position.XY).Contains(_.Position.XY));
                        // We have the correct type, try to eat him
                        if (opponent != null && opponent.GetBetterType() == pac.PacType)
                        {
                            pac.Command = "MOVE";
                            pac.CommandArgs = opponent.Position;
                            pac.CommandMessage = $"Eat {opponent.Id}!";
                        }
                        // Switch to correct type to eat him
                        else if (opponent != null && pac.AbilityCooldown == 0)
                        {
                            pac.Command = "SWITCH";
                            pac.CommandArgs = opponent.GetBetterType();
                        }
                        // Run away
                        else if (opponent != null && opponent.Position.Manhattan(pac.Position) <= 4)
                        {
                            var evadingDirection = pac.Position.GetDirectionTo(opponent.Position).Value.GetOpposite();
                            pac.Command = "MOVE";
                            for (int i = 1; i < 100; i++)
                            {
                                var tile = map.GetAdjacent(pac.Position, evadingDirection, i);
                                if (tile?.IsWall == false)
                                {
                                    pac.CommandArgs = tile.Position;
                                    break;
                                }

                                if (tile == null)
                                {
                                    pac.CommandArgs = pac.Position; // No escape, don't move and pray
                                    break;
                                }
                            }

                            pac.CommandMessage = $"Move away from {opponent.Id}";
                        }
                        else
                        {
                            // Use SPEED
                            if (pac.AbilityCooldown == 0)
                            {
                                pac.Command = "SPEED";
                            }
                            // Had collision
                            else if (pac.Command == "MOVE" && pac.Position.XY == pac.PreviousPosition.XY)
                            {
                                var collidingPac = pacs.FirstOrDefault(_ =>
                                    !(_.Id == pac.Id && _.IsMine) && _.Position.Manhattan(pac.Position) <= 2);
                                if (collidingPac?.IsMine == true)
                                {
                                    pac.Command = "MOVE";
                                    pac.CommandArgs = map.GetRandomTile().Position;
                                    pac.CommandMessage = "Collision";
                                }
                                else if (collidingPac?.IsMine == false)
                                {

                                    pac.Command = "SWITCH";
                                    pac.CommandArgs = collidingPac.GetBetterType();
                                }
                            }
                            // Simple MOVE
                            else
                            {
                                var closestPellet = map.Tiles.Where(_ => _.Pellet != null)
                                    .Where(_ => !pacs.Where(p => p.IsMine).Select(p => p.CommandArgs)
                                        .Contains(_)) // Exclude destinations already set for other pacs
                                    .OrderByDescending(_ => _.Pellet.Value).ThenBy(_ => _.Position.Manhattan(pac.Position))
                                    .FirstOrDefault();

                                pac.Command = "MOVE";
                                pac.CommandArgs =
                                    closestPellet?.Position ?? map.GetRandomTile().Position;
                            }
                        }
                    }

                    Console.WriteLine($"{string.Join("|", pacs.Where(_ => _.IsMine).Select(_ => $"{_.GetFullCommand()}"))}");
                }
            }
        }

        public class Tile : AStarSearch.Location
        {
            public Tile(Position position, bool isWall) : base(position, isWall)
            {
            }

            public bool Unknown { get; set; } = true;

            public Pellet Pellet { get; set; }
        }

        public class Map
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public List<Tile> Tiles { get; set; } = new List<Tile>();

            public Tile GetTile(Position position)
            {
                return Tiles.First(_ => _.Position.XY == position.XY);
            }

            public Tile GetRandomTile()
            {
                return Tiles.OrderBy(_ => Guid.NewGuid()).First();
            }

            public Tile GetAdjacent(Position position, Direction direction, int distance = 1)
            {
                int xModifier;
                switch (direction)
                {
                    case Direction.Est:
                        xModifier = distance;
                        break;
                    case Direction.West:
                        xModifier = -distance;
                        break;
                    default:
                        xModifier = 0;
                        break;
                }

                int yModifier;
                switch (direction)
                {
                    case Direction.South:
                        yModifier = distance;
                        break;
                    case Direction.North:
                        yModifier = -distance;
                        break;
                    default:
                        yModifier = 0;
                        break;
                }

                return Tiles.FirstOrDefault(_ => _.Position.X == position.X + xModifier && _.Position.Y == position.Y + yModifier);
            }

            public IEnumerable<Tile> GetTilesInSightFromAllDirections(Position position, int depth = 1000)
            {
                return GetTilesInSight(position, Direction.West, depth)
                    .Union(GetTilesInSight(position, Direction.North, depth))
                    .Union(GetTilesInSight(position, Direction.Est, depth))
                    .Union(GetTilesInSight(position, Direction.South, depth));
            }

            public List<Tile> GetTilesInSight(Position position, Direction direction, int depth = 1000)
            {
                var tiles = new List<Tile>();

                for (int i = 1; i < depth; i++)
                {
                    var tile = GetAdjacent(position, direction, i);
                    if (tile?.IsWall != false) break;
                    tiles.Add(tile);
                }
                return tiles;
            }
        }

        public enum PacType
        {
            ROCK,
            PAPER,
            SCISSORS
        }

        public class Pac
        {
            public Position Position { get; set; }
            public Position PreviousPosition { get; set; }

            public bool IsMine { get; set; }

            public string Command { get; set; }
            public object CommandArgs { get; set; }
            public string CommandMessage { get; set; }
            public int Id { get; set; }

            public PacType PacType { get; set; }
            public bool IsAlive { get; set; }
            public int SpeedTurnsLeft { get; set; }
            public int AbilityCooldown { get; set; }
            public bool IsVisible { get; set; }

            public string GetFullCommand()
            {
                var result = $"{Command} {Id} {CommandArgs} {CommandMessage}";
                
                return result;
            }

            public PacType GetBetterType()
            {
                switch (PacType)
                {
                    case PacType.ROCK:
                        return PacType.PAPER;
                    case PacType.PAPER:
                        return PacType.SCISSORS;
                    case PacType.SCISSORS:
                        return PacType.ROCK;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public class Pellet
        {
            public int Value { get; set; }
        }

        public class Position
        {
            public int X, Y;

            public string XY => X + " " + Y;
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

            public Direction? GetDirectionTo(Position destination)
            {
                if (destination.X > X)
                    return Direction.Est;
                if (destination.X < X)
                    return Direction.West;
                if (destination.Y > Y)
                    return Direction.South;
                if (destination.Y < Y)
                    return Direction.North;
                return null;
            }
        }

        public enum Direction
        {
            North,
            South,
            Est,
            West
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

        public class AStarSearch
        {
            public List<Location> Map { get; }

            public AStarSearch(List<Location> map)
            {
                Map = map;
            }

            public Route Compute(Position start, Position end)
            {
                var openList = new List<Location>();
                var closedList = new List<Location>();
                var hasFoundPath = false;
                var movementCost = 0;

                // start by adding the original position to the open list
                var startLocation = Map.FirstOrDefault(_ => _.Position.XY == start.XY);
                if (startLocation != null)
                    openList.Add(startLocation);

                var endLocation = Map.FirstOrDefault(_ => _.Position.XY == end.XY);

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
                    if (current.Position.XY == end.XY)
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
                            adjacentSquare.CostToDestination = adjacentSquare.Position.Manhattan(end);
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
                var currentResult = endLocation;

                while (currentResult != null)
                {
                    result.Add(currentResult);
                    currentResult = currentResult.Parent;
                }

                result.Reverse();
                result.Remove(startLocation);

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

                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y - 1), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X && _.Position.Y == Position.Y + 1), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X - 1 && _.Position.Y == Position.Y), proposedLocations);
                    AddIfNotNull(map.FirstOrDefault(_ => _.Position.X == Position.X + 1 && _.Position.Y == Position.Y), proposedLocations);

                    return proposedLocations;
                }

                public static void AddIfNotNull<T>(T item, List<T> list)
                {
                    if (item != null)
                        list.Add(item);
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
    }

    public static class DirectionExtension
    {
        public static Player.Direction GetOpposite(this Player.Direction direction)
        {
            switch (direction)
            {
                case Player.Direction.North:
                    return Player.Direction.South;
                case Player.Direction.West:
                    return Player.Direction.Est;
                case Player.Direction.Est:
                    return Player.Direction.West;
                case Player.Direction.South:
                    return Player.Direction.North;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}