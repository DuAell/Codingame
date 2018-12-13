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
            game.LaunchTurn("0\n0110 1011 1101 0110 1001 0011 1111\n0110 1011 0110 1010 0011 0110 0111\n0101 1010 0101 0111 1010 0110 1110\n1011 0101 1101 0101 0111 0101 1110\n1011 1001 1010 1101 0101 1010 0101\n1101 1001 1100 1010 1001 1110 1001\n1111 1100 0110 1001 0111 1110 1001\n12 1 1 0111\n12 5 4 1101\n24\nSHIELD 3 4 0\nCANE 5 1 0\nBOOK 1 0 1\nPOTION 0 6 1\nCANDY 4 1 1\nFISH 2 0 0\nKEY 2 2 1\nSWORD -2 -2 1\nBOOK 5 6 0\nCANDY 2 5 0\nPOTION 6 0 0\nARROW 0 1 0\nDIAMOND 0 2 1\nSCROLL 4 5 1\nMASK 1 2 0\nARROW 6 5 1\nSHIELD 3 2 1\nMASK 5 4 1\nKEY 4 4 0\nFISH 4 6 1\nSCROLL 2 1 0\nCANE 1 5 1\nDIAMOND 6 4 0\nSWORD -1 -1 0\n6\nDIAMOND 0\nBOOK 0\nCANE 0\nDIAMOND 1\nBOOK 1\nCANE 1");
        }
    }
}
