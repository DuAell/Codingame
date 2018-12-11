using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmasRush.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var game = new Game();
            game.LaunchTurn("1\n1001 0110 1001 1010 0110 0110 0111\n1011 1011 0110 1110 0111 1101 1001\n1110 1010 0111 0101 1010 0111 0110\n1010 1101 1001 1101 1001 1001 1111\n1110 1010 1010 1001 1111 0110 1010\n0111 1010 1011 0111 0110 0110 1111\n1101 1010 1001 1101 1010 0101 1101\n1 0 4 1001\n1 6 6 0110\n2\nARROW 1 3 1\nARROW 6 0 0\n2\nARROW 0\nARROW 1");
        }
    }
}
