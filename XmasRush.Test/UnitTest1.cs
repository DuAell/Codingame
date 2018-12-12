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
            game.LaunchTurn("1\n1111 1010 0110 1010 0110 0101 1101\n1010 1110 0111 1101 1010 0011 1110\n1101 1010 1001 0110 0011 0111 1001\n1001 1101 1011 0110 1110 0111 0110\n1010 0111 0111 0101 1101 1100 1001\n1011 1100 1010 0110 1101 1011 1010\n1001 1010 1111 1001 0111 0101 1001\n6 4 3 0110\n6 4 6 1010\n12\nBOOK 4 6 0\nPOTION 0 2 0\nMASK 1 3 0\nMASK 5 3 1\nBOOK 6 0 1\nCANDY 2 4 0\nDIAMOND 0 5 1\nPOTION 1 4 1\nDIAMOND 6 1 0\nCANDY 3 1 1\nARROW 2 1 0\nARROW 4 5 1\n2\nCANDY 0\nCANDY 1");
        }
    }
}
