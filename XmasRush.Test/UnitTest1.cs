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
            var game = new Game("0\n1001 0110 1001 1010 0011 0101 1011\n0011 1011 1110 0110 1010 1111 0111\n1001 0110 1110 1011 1101 1101 1010\n0110 1010 0011 1011 0101 1110 1100\n1010 0111 0111 1110 1011 1001 0110\n1101 1111 1010 1001 1011 1110 1100\n1010 1110 0101 1100 1010 0110 1001\n1 1 0 1010\n1 5 6 1010\n2\nMASK 3 2 0\nMASK 3 4 1\n2\nMASK 0\nMASK 1");
            game.LaunchTurn();
        }
    }
}
