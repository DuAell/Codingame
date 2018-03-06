using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int maxCheckpointDist = 0;
        var checkpoints = new List<Point>();
        bool allCheckpointsKnown = false;
        Point boostAtCheckpoint = new Point();
        bool isBoostAvailable = true;
        bool isBoost = false;
        int thrust = 100;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);

            var checkpoint = new Point(nextCheckpointX, nextCheckpointY);
            if (checkpoints.Count > 1 && checkpoints.First() == checkpoint)
                allCheckpointsKnown = true;
            else if (!checkpoints.Contains(checkpoint))
            {
                checkpoints.Add(checkpoint);
                maxCheckpointDist = Math.Max(nextCheckpointDist, maxCheckpointDist);
                if (maxCheckpointDist == nextCheckpointDist)
                    boostAtCheckpoint = checkpoint;
            }
            Console.Error.WriteLine("allCheckpointsKnown: " + allCheckpointsKnown);
            Console.Error.WriteLine("nextCheckpoint: " + checkpoint);
            Console.Error.WriteLine("boostAtCheckpoint: " + boostAtCheckpoint);
            Console.Error.WriteLine("nextCheckpointDist: " + nextCheckpointDist);
            Console.Error.WriteLine("nextCheckpointAngle: " + nextCheckpointAngle);
            Console.Error.WriteLine("maxCheckpointDist: " + maxCheckpointDist);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            thrust = 100 - (Math.Abs(nextCheckpointAngle) / 2);
            if (Math.Abs(nextCheckpointAngle) > 5 && nextCheckpointDist < 3000 && thrust > 50)
            {
                thrust = 50;
                Console.Error.WriteLine("Freinage d'urgence");
            }
            if (Math.Abs(nextCheckpointAngle) > 80)
            {
                thrust = 0;
                Console.Error.WriteLine("Alignement");
            }
            if (thrust > 100)
                thrust = 100;
            if (thrust < 0)
                thrust = 0;

            if (allCheckpointsKnown && isBoostAvailable && boostAtCheckpoint == checkpoint && Math.Abs(nextCheckpointAngle) < 20)
            {
                isBoost = true;
                isBoostAvailable = false;
            }

            // You have to output the target position
            // followed by the power (0 <= thrust <= 100)
            // i.e.: "x y thrust"
            Console.WriteLine(nextCheckpointX + " " + nextCheckpointY + " " + (isBoost ? "BOOST" : thrust.ToString()));
            isBoost = false;
        }
    }

    private static double GetDistance(Point a, Point b)
    {
        return Math.Sqrt(Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2));
    }
}