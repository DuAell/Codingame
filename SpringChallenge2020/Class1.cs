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
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]); // size of the grid
            int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
            for (int i = 0; i < height; i++)
            {
                string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
            }

            // game loop
            while (true)
            {
                var pacs = new List<Pac>();

                inputs = Console.ReadLine().Split(' ');
                int myScore = int.Parse(inputs[0]);
                int opponentScore = int.Parse(inputs[1]);
                int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight
                for (int i = 0; i < visiblePacCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                    bool mine = inputs[1] != "0"; // true if this pac is yours
                    int x = int.Parse(inputs[2]); // position in the grid
                    int y = int.Parse(inputs[3]); // position in the grid
                    string typeId = inputs[4]; // unused in wood leagues
                    int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                    int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

                    pacs.Add(new Pac(x, y) {Id = pacId, IsMine = mine});
                }
                int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
                var visiblePellets = new List<Pellet>();

                for (int i = 0; i < visiblePelletCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    visiblePellets.Add(
                        new Pellet(int.Parse(inputs[0]), int.Parse(inputs[1]))
                            {Value = int.Parse(inputs[2])}
                    );
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                foreach (var pac in pacs.Where(_ => _.IsMine))
                {
                    var closest = visiblePellets.Where(_ =>
                            _.Value == visiblePellets.Max(v => v.Value) &&
                            !pacs.Where(p => p.IsMine).Select(p => p.Destination).Contains(_)) // Exclude destinations already set for other pacs
                        .OrderBy(_ => _.Manhattan(pac)).First();

                    pac.Destination = closest;
                }

                Console.WriteLine($"{string.Join("|", pacs.Where(_ => _.IsMine).Select(_ => $"MOVE {_.Id} {_.Destination}"))}");
            }
        }

        public class Pac : Position
        {
            public Pac(int x, int y) : base(x, y)
            {
            }

            public bool IsMine { get; set; }

            public Position Destination { get; set; }
            public int Id { get; set; }
        }

        public class Pellet : Position
        {
            public Pellet(int x, int y) : base(x, y)
            {
            }

            public int Value { get; set; }
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
    }
}