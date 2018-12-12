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
            game.LaunchTurn("0\n0110 0110 0111 1001 1101 1001 0111\n1010 1010 0110 1001 1010 1111 1111\n0011 1101 1001 1010 0110 0011 1101\n1110 1101 0111 0011 0101 0111 1001\n1010 1010 1010 1100 0011 1100 0111\n1010 0110 0111 0111 1100 0111 1101\n0101 1010 1100 1101 0101 1101 1011\n4 2 5 1101\n4 5 6 1010\n8\nFISH 6 1 1\nSHIELD 6 5 0\nPOTION -1 -1 0\nMASK 3 6 1\nFISH 5 1 0\nSHIELD 5 3 1\nMASK 2 0 0\nPOTION 6 4 1\n2\nSHIELD 0\nSHIELD 1");
        }
    }
}
