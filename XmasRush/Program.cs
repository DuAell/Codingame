using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace XmasRush
{
    public enum TurnType
    {
        Push,
        Move
    }

    /**
     * Help the Christmas elves fetch presents in a magical labyrinth!
     **/
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Launch();
        }        
    }

    public class Game
    {
        public Game(string debugData = null)
        {
            InputProcessor = new InputProcessor(debugData);
        }

        public InputProcessor InputProcessor { get; }

        public void Launch()
        {
            // game loop
            while (true)
            {
                LaunchTurn();
            }
        }

        public void LaunchTurn()
        {
            var turnData = InputProcessor.ProcessInput();

            if (turnData.TurnType == TurnType.Push)
                Console.WriteLine("PUSH 3 RIGHT"); // PUSH <id> <direction> | MOVE <direction> | PASS
            else
                Console.WriteLine("PASS");
        }
    }

    public class TurnData
    {
        public TurnType TurnType { get; set; }
    }

    public class InputProcessor
    {
        public InputProcessor(string debugData = null)
        {
            if (debugData != null)
                _stringReader = new StringReader(debugData);
        }

        public List<string> InputLines { get; } = new List<string>();

        public TurnData ProcessInput()
        {
            InputLines.Clear();

            var turnData = new TurnData();

            string[] inputs;

            turnData.TurnType = int.Parse(ReadLine()) == 0 ? TurnType.Push : TurnType.Move;

            for (int i = 0; i < 7; i++)
            {
                inputs = ReadLine().Split(' ');
                for (int j = 0; j < 7; j++)
                {
                    string tile = inputs[j];
                }
            }
            for (int i = 0; i < 2; i++)
            {
                inputs = ReadLine().Split(' ');
                int numPlayerCards = int.Parse(inputs[0]); // the total number of quests for a player (hidden and revealed)
                int playerX = int.Parse(inputs[1]);
                int playerY = int.Parse(inputs[2]);
                string playerTile = inputs[3];
            }
            int numItems = int.Parse(ReadLine()); // the total number of items available on board and on player tiles
            for (int i = 0; i < numItems; i++)
            {
                inputs = ReadLine().Split(' ');
                string itemName = inputs[0];
                int itemX = int.Parse(inputs[1]);
                int itemY = int.Parse(inputs[2]);
                int itemPlayerId = int.Parse(inputs[3]);
            }
            int numQuests = int.Parse(ReadLine()); // the total number of revealed quests for both players
            for (int i = 0; i < numQuests; i++)
            {
                inputs = ReadLine().Split(' ');
                string questItemName = inputs[0];
                int questPlayerId = int.Parse(inputs[1]);
            }

            Console.Error.WriteLine(string.Join("\\n", InputLines));

            return turnData;
        }

        private readonly StringReader _stringReader;
        private string ReadLine()
        {
            if (_stringReader != null)
            {
                return _stringReader.ReadLine();
            }
            else
            {
                var line = Console.ReadLine();
                InputLines.Add(line);
                return line;
            }
        }
    }
}
