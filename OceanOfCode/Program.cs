using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace OceanOfCode
{

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class OceanOfCode
    {
        static void Main(string[] args)
        {
            Game.Play();
        }
    }

    public static class Game
    {
        public static Map Map { get; set; }

        public static Player Me { get; set; }

        public static Player Ennemy { get; set; }

        public static int TurnNumber { get; set; } = 0;

        public static int LastSonarSectorResearched { get; set; }

        public static void Play(string initData = null, string turnData = null, string startPositionData = null)
        {
            var initDataInputProcessor = new InputProcessor(initData);
            var turnDataInputProcessor = new InputProcessor(turnData);
            Initialize(initDataInputProcessor);

            // Output starting position
            var startingPosition = Map.Tiles.Where(x => x.IsWater).OrderBy(x => Guid.NewGuid()).First().ToString();
            if (startPositionData != null)
                startingPosition = startPositionData;

            //startingPosition = "11 8";

            Console.WriteLine(startingPosition);

            while (true)
            {
                initDataInputProcessor.WriteDebugData();
                GameTurn(turnDataInputProcessor);
            }
        }

        public static void Initialize(InputProcessor inputProcessor)
        {
            var inputs = inputProcessor.ReadLine().Split(' ');

            Map = new Map {Width = int.Parse(inputs[0]), Height = int.Parse(inputs[1])};

            for (var row = 0; row < Map.Height; row++)
            {
                var line = inputProcessor.ReadLine();
                for (var column = 0; column < Map.Width; column++)
                {
                    Map.Tiles.Add(new Tile(column, row) { IsWater = line[column] == '.' });
                }
            }

            Me = new Player
            {
                Id = int.Parse(inputs[2]),
                TileInfos = new List<TileInfo>(Map.Tiles.Select(_ => new TileInfo(_)))
            };

            Ennemy = new Player
            {
                TileInfos = new List<TileInfo>(Map.Tiles.Select(_ => new TileInfo(_)))
            };
        }

        public static void GameTurn(InputProcessor inputProcessor)
        {
            TurnNumber += 2;
            var inputs = inputProcessor.ReadLine().Split(' ');
            Me.Tile = Map.Tiles.First(_ => _.X == int.Parse(inputs[0]) && _.Y == int.Parse(inputs[1]));
            Me.GetTileInfo(Me.Tile).HasBeenVisited = true;
            Me.Life = int.Parse(inputs[2]);
            Ennemy.Life = int.Parse(inputs[3]);
            int torpedoCooldown = int.Parse(inputs[4]);
            int sonarCooldown = int.Parse(inputs[5]);
            int silenceCooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            string sonarResult = inputProcessor.ReadLine();
            string opponentOrders = inputProcessor.ReadLine();

            inputProcessor.WriteDebugData();

            UpdateEnnemyPosition(opponentOrders, sonarResult);

            ShowMapOfPossibleEnnemyPositions();

            var ennemyPossiblePositions = Ennemy.TileInfos.Where(_ => _.CanBeThere).ToList();

            var charge = "TORPEDO";
            if (Me.TorpedoCharges >= 3)
                if (ennemyPossiblePositions.Count > 20)
                    charge = "SONAR";
                else
                    charge = "SILENCE";

            var outputs = new List<string>();

            var destination = ennemyPossiblePositions.First();
            var astar = new AStarSearch(Me.TileInfos.Select(_ => new AStarSearch.Location(_.Tile, !_.Tile.IsWater || _.HasBeenVisited)).ToList());
            var route = astar.Compute(Me.Tile, destination.Tile);
            if (Enum.GetValues(typeof(Direction)).Cast<Direction>().All(_ => !Me.CanGo(_)))
            {
                // Can't go anywhere
                outputs.Add("SURFACE");
                Me.TileInfos.ForEach(_ => _.HasBeenVisited = false);
            }
            else if (!route.IsValid)
            {
                Console.Error.WriteLine("No way to go to the ennemy, going to random direction");
                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    if (Me.CanGo(direction))
                    {
                        outputs.Add(Move(direction, charge));
                        break;
                    }
                }
            }
            //else if (route.Positions.Count <= 2)
            //{
            //    var moveDone = false;
            //    var nextPosition = route.Positions.FirstOrDefault();

            //    Console.Error.WriteLine($"Too close from the ennemy in {destination.Tile}");
            //    foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            //    {
            //        // Stay away from the ennemy
            //        if (route.Positions.Any() && direction == Me.Tile.GetDirectionTo(nextPosition))
            //            continue;
            //        if (Me.CanGo(direction))
            //        {
            //            outputs.Add(Move(direction, charge));
            //            moveDone = true;
            //            break;
            //        }
            //    }

            //    Console.Error.WriteLine($"Can't move away from the ennemy in {destination.Tile}");
            //    if (!moveDone && nextPosition != null)
            //    {
            //        var direction = Me.Tile.GetDirectionTo(nextPosition);
            //        if (direction != null && Me.CanGo(direction.Value))
            //            outputs.Add(Move(direction.Value, charge));
            //    }
            //}
            else
            {
                // TODO : can be improved : can move and silence the same turn, but needs to manually change my position
                if (silenceCooldown <= 0 && Me.SilenceCharges >= 6)
                {
                    var silenceDone = false;
                    for (var distance = 4; distance > 0; distance--)
                    {
                        if (silenceDone) break;
                        foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                        {
                            if (Me.CanGo(direction, distance))
                            {
                                //outputs.Clear();
                                outputs.Add($"SILENCE {direction.GetValue()} {distance}");
                                Me.SilenceCharges = 0;

                                for (int i = 1; i <= distance; i++)
                                {
                                    Me.GetTileInfo(Map.GetAdjacent(Me.Tile, direction, i)).HasBeenVisited = true;
                                }

                                silenceDone = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Approching the ennemy in {destination.Tile}");
                    var nextMove = route.Positions.First();
                    var nextDirection = Me.Tile.GetDirectionTo(nextMove);
                    if (nextDirection != null)
                    {
                        outputs.Add(Move(nextDirection.Value, charge));
                    }
                }
            }

            if (torpedoCooldown <= 0 && ennemyPossiblePositions.Count < 10 && Me.TorpedoCharges >= 3)
            {
                // No matter if I shot myself, I will kill him
                if (ennemyPossiblePositions.Count == 1 && Me.Life > 1 && Ennemy.Life <= 2 && ennemyPossiblePositions.First().Tile.Manhattan(Me.Tile) <= 4)
                {
                    outputs.Add($"TORPEDO {Ennemy.TileInfos.First(_ => _.CanBeThere).Tile}");
                    Me.TorpedoCharges = 0;
                }
                else
                {
                    var fireTarget = ennemyPossiblePositions.Where(_ => _.Tile.Manhattan(Me.Tile) <= 4 && _.Tile.Manhattan(Me.Tile) > 2).OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                    if (fireTarget != null)
                    {
                        outputs.Add($"TORPEDO {fireTarget.Tile}");
                        Me.TorpedoCharges = 0;
                    }
                }
            }

            if (sonarCooldown <= 0 && ennemyPossiblePositions.Count > 20 && Me.SonarCharges >= 4)
            {
                var maxTiles = 0;
                LastSonarSectorResearched = 0;
                for (var i = 1; i <= 9; i++)
                {
                    var count = Map.GetTilesOfSector(i).Count(t => Ennemy.GetTileInfo(t).CanBeThere);
                    if (count > maxTiles)
                        LastSonarSectorResearched = i;
                }
                outputs.Add($"SONAR {LastSonarSectorResearched}");
            }

            // No move possible, then surface
            if (!outputs.Any())
            {
                outputs.Add("SURFACE");
                Me.TileInfos.ForEach(_ => _.HasBeenVisited = false);
                Console.Error.WriteLine("No outputs done, go to the surface");
            }

            Console.WriteLine(string.Join(" | ", outputs));
        }

        private static string Move(Direction direction, string charge)
        {
            var result = $"MOVE {direction.GetValue()} {charge}";
            if (charge == "TORPEDO") Me.TorpedoCharges++;
            if (charge == "SILENCE") Me.SilenceCharges++;
            if (charge == "SONAR") Me.SonarCharges++;
            return result;
        }

        private static void ShowMapOfPossibleEnnemyPositions()
        {
            for (var row = 0; row <= Map.Height - 1; row++)
            {
                var line = string.Empty;
                for (var column = 0; column <= Map.Width - 1; column++)
                {
                    var tileInfo = Ennemy.GetTileInfo(Map.Tiles.First(_ => _.X == column && _.Y == row));
                    if (!tileInfo.Tile.IsWater)
                        line += "x";
                    else if (tileInfo.CanBeThere)
                        line += "?";
                    else
                        line += ".";
                }

                line += " | ";

                for (var column = 0; column <= Map.Width - 1; column++)
                {
                    var tileInfo = Me.GetTileInfo(Map.Tiles.First(_ => _.X == column && _.Y == row));
                    if (tileInfo.Tile == Me.Tile)
                        line += "O";
                    else if (!tileInfo.Tile.IsWater)
                        line += "x";
                    else if (tileInfo.HasBeenVisited)
                        line += "V";
                    else
                        line += ".";
                }

                Console.Error.WriteLine(line);
            }
        }

        private static void UpdateEnnemyPosition(string opponentOrders, string sonarResult)
        {
            if (sonarResult == "Y")
                Ennemy.TileInfos.Except(Map.GetTilesOfSector(LastSonarSectorResearched).Select(Ennemy.GetTileInfo)).ToList().ForEach(_ => _.CanBeThere = false);
            else if (sonarResult == "N")
                Map.GetTilesOfSector(LastSonarSectorResearched).Select(Ennemy.GetTileInfo).ToList().ForEach(_ => _.CanBeThere = false);

            var orders = opponentOrders.Split('|').Select(_ => _.Trim()).ToList();

            foreach (var order in orders)
            {
                if (order.StartsWith("TORPEDO "))
                    UpdateEnnemyPosition_Torpedo(order.Replace("TORPEDO ", string.Empty));
                else if (order.StartsWith("SURFACE "))
                    UpdateEnnemyPosition_Surface(order.Replace("SURFACE ", string.Empty));
                else if (order.StartsWith("SILENCE"))
                    UpdateEnnemyPosition_Silence();
                else if (order.StartsWith("MOVE "))
                    UpdateEnnemyPosition_MoveTheMap(GetDirection(order.Substring("MOVE ".Length, 1)));
            }
            
            Console.Error.WriteLine($"Ennemy can be in {Ennemy.TileInfos.Count(_ => _.CanBeThere)} positions");
        }

        private static Direction GetDirection(string letter)
        {
            switch (letter)
            {
                case "N":
                    return Direction.North;
                case "S":
                    return Direction.South;
                case "E":
                    return Direction.Est;
                case "W":
                    return Direction.West;
                default:
                    throw new ArgumentOutOfRangeException(nameof(letter));
            }
        }

        private static void UpdateEnnemyPosition_MoveTheMap(Direction direction)
        {
            var oppositeDirection = direction.GetOpposite();
            
            var canBeThereToFalse = new List<TileInfo>();
            var canBeThereToTrue = new List<TileInfo>();

            // Update all tiles with an adjacent of opposite direction with CanBeThere false to false => If going north : Remove bottom line
            canBeThereToFalse = Ennemy.TileInfos.Where(_ =>
                _.CanBeThere &&
                Ennemy.GetTileInfo(Map.GetAdjacent(_.Tile, oppositeDirection))?.CanBeThere != true).ToList();

            // Update all tiles with an adjacent of opposite direction with CanBeThere true to true => If going north : Add upper line
            canBeThereToTrue = Ennemy.TileInfos.Where(_ => !_.CanBeThere &&
                                                           _.Tile.IsWater &&
                                                           Ennemy.GetTileInfo(Map.GetAdjacent(_.Tile, oppositeDirection))?.CanBeThere == true).ToList();
            
            canBeThereToFalse.ForEach(_ => _.CanBeThere = false);
            canBeThereToTrue.ForEach(_ => _.CanBeThere = true);
        }

        private static void UpdateEnnemyPosition_Torpedo(string position)
        {
            Ennemy.TileInfos.Where(_ => _.Tile.Manhattan(new Position(int.Parse(position.Split(' ')[0]), int.Parse(position.Split(' ')[1]))) > 4).ToList().ForEach(_ => _.CanBeThere = false);
        }

        private static void UpdateEnnemyPosition_Surface(string sector)
        {
            var sectorTiles = Map.GetTilesOfSector(int.Parse(sector)).ToList();
            Ennemy.TileInfos.Where(_ => !sectorTiles.Contains(_.Tile)).ToList().ForEach(_ => _.CanBeThere = false);
        }
        private static void UpdateEnnemyPosition_Silence()
        {
            foreach (var tileInfo in Ennemy.TileInfos.Where(_ => _.CanBeThere).ToList())
            {
                Ennemy.TileInfos.Where(_ => (Math.Abs(_.Tile.X - tileInfo.Tile.X) <= 4 && _.Tile.Y == tileInfo.Tile.Y || _.Tile.X == tileInfo.Tile.X && Math.Abs(_.Tile.Y - tileInfo.Tile.Y) <= 4) && _.Tile.IsWater).ToList().ForEach(_ => _.CanBeThere = true);
            }
        }
    }

    public static class DirectionExtension
    {
        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.West:
                    return Direction.Est;
                case Direction.Est:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static string GetValue(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return "N";
                case Direction.South:
                    return "S";
                case Direction.Est:
                    return "E";
                case Direction.West:
                    return "W";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }

    public class Player
    {
        public int Id { get; set; }
        public List<TileInfo> TileInfos { get; set; } = new List<TileInfo>();

        public TileInfo GetTileInfo(Tile tile)
            => TileInfos.FirstOrDefault(_ => _.Tile == tile);

        public int Life { get; set; }
        public int TorpedoCharges { get; set; }
        public int SilenceCharges { get; set; }
        public int SonarCharges { get; set; }
        public Tile Tile { get; set; }

        public bool CanGo(Direction direction, int distance = 1)
        {
            var result = true;
            for (int i = 1; i <= distance; i++)
            {
                var expectedDestination = Game.Map.GetAdjacent(Tile, direction, i);
                if (expectedDestination == null) return false;
                result &= expectedDestination.IsWater && !GetTileInfo(expectedDestination).HasBeenVisited;
                Console.Error.WriteLine($"{(result ? "Can" : "Cannot")} go {direction}(distance {distance}) ({expectedDestination}). Water: {expectedDestination.IsWater} / Already visited: {GetTileInfo(expectedDestination).HasBeenVisited}");
            }

            //var expectedDestination = Game.Map.GetAdjacent(this, direction, distance);
            //var result = expectedDestination?.IsWater == true && !GetTileInfo(expectedDestination).HasBeenVisited;

            //Console.Error.WriteLine($"{(result ? "Can" : "Cannot")} go {direction}(distance {distance}) ({expectedDestination}). Water: {expectedDestination?.IsWater} / Already visited: {GetTileInfo(expectedDestination)?.HasBeenVisited}");

            return result;
        }
    }

    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<Tile> Tiles { get; set; } = new List<Tile>();

        public IEnumerable<Tile> GetTilesOfSector(int sector)
        {
            var xMod = 0;
            var yMod = 0;

            switch (sector)
            {
                case 4:
                case 5:
                case 6:
                    yMod = 1;
                    break;
                case 7:
                case 8:
                case 9:
                    yMod = 2;
                    break;
            }

            switch (sector)
            {
                case 2:
                case 5:
                case 8:
                    xMod = 1;
                    break;
                case 3:
                case 6:
                case 9:
                    xMod = 2;
                    break;
            }

            return Tiles.Where(_ => _.X >= 0 + 5 * xMod && _.X <= 4 + 5 * xMod && _.Y >= 0 + 5 * yMod && _.Y <= 4 + 5 * yMod);
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

            return Tiles.FirstOrDefault(_ => _.X == position.X + xModifier && _.Y == position.Y + yModifier);
        }
    }

    public class TileInfo
    {
        public Tile Tile { get; }

        public TileInfo(Tile tile)
        {
            Tile = tile;
            if (!tile.IsWater)
                CanBeThere = false;
        }

        public bool HasBeenVisited { get; set; }

        public bool CanBeThere { get; set; } = true;
    }

    public enum Direction
    {
        North,
        South,
        Est,
        West
    }

    public class Tile : Position
    {
        public Tile(int x, int y) : base(x, y)
        {
        }

        public bool IsWater { get; set; }
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

        public string XY => X + " " + Y;

        public override string ToString()
        {
            return XY;
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
