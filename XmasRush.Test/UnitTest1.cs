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
            game.LaunchTurn("0\n1101 0011 0011 1001 1101 0111 0111\n0110 1111 0111 0110 0111 1001 1010\n0101 1001 1010 0101 0101 0111 1001\n1010 1101 1110 1001 1011 0110 0111\n1001 0110 0101 0110 1010 1111 0110\n1101 1101 1101 1101 1101 1100 1010\n0110 1010 0111 1010 1100 1001 0101\n10 0 5 1010\n12 4 2 0111\n22\nDIAMOND 3 6 1\nCANE 2 6 1\nPOTION 6 2 1\nBOOK 1 1 0\nBOOK 5 4 1\nCANDY 5 1 1\nSWORD 4 2 1\nSWORD 2 4 0\nKEY 0 2 0\nARROW 2 5 1\nMASK 1 5 0\nKEY 6 6 1\nSCROLL 3 0 1\nARROW 4 1 0\nFISH 3 3 1\nDIAMOND 6 1 0\nPOTION 0 6 0\nFISH 3 1 0\nMASK 5 0 1\nCANE 4 0 0\nSHIELD 6 3 1\nCANDY 1 4 0\n6\nKEY 0\nDIAMOND 0\nCANDY 0\nSCROLL 1\nKEY 1\nDIAMOND 1");
        }
    }
}
