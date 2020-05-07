using System.Collections.Generic;
using System.Linq;

namespace Helper
{
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
