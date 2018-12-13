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
            game.LaunchTurn("1\n0110 0111 0111 0111 1001 1001 1111\n1111 0111 1001 1011 0110 1001 0111\n0110 0110 1010 1010 0101 1011 1010\n1010 1001 1010 0110 0110 1010 1101\n1110 0111 0101 1101 1001 1101 0110\n1010 1010 0110 1001 1010 0111 1010\n1110 1101 1101 1101 1001 1111 0110\n5 4 4 1001\n5 0 4 1101\n10\nKEY 0 2 0\nCANDY 3 3 1\nSWORD 5 3 1\nCANE 6 5 1\nSCROLL 2 4 1\nPOTION 0 6 0\nSCROLL 4 2 0\nCANDY 3 5 0\nPOTION 3 1 1\nMASK 1 2 0\n6\nKEY 0\nMASK 0\nPOTION 0\nCANE 1\nSWORD 1\nPOTION 1");
        }
    }
}
