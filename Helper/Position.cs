using System;
using System.Collections.Generic;
using System.Linq;

namespace Helper
{
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
                return Direction.East;
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
        East,
        West
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
                    return Direction.East;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
