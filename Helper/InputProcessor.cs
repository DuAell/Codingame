using System;
using System.Collections.Generic;

namespace Helper
{
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
