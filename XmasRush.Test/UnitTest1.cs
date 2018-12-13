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
            game.LaunchTurn("1\n0110 1101 1010 1100 1100 0011 1010\n0011 0111 0101 0011 0101 0111 1010\n0110 1110 1010 0011 0101 0011 1110\n1011 1100 0101 0101 0111 1101 1010\n0111 1101 1100 1001 1011 1010 1100\n1010 1010 0101 1101 0111 1101 1001\n0110 1010 0101 0110 0101 1001 1010\n11 2 4 1001\n12 4 5 0101\n23\nSWORD 3 6 0\nPOTION 6 2 0\nCANE 5 0 1\nARROW 1 4 1\nMASK 0 6 0\nSCROLL 4 1 0\nCANDY 0 5 0\nPOTION 0 3 1\nKEY 0 4 0\nARROW 5 1 0\nBOOK 4 5 1\nKEY 5 5 1\nDIAMOND 6 1 1\nDIAMOND 6 3 0\nFISH 5 3 1\nBOOK 1 0 0\nFISH 1 1 0\nCANDY 6 0 1\nSCROLL 2 6 1\nSHIELD 4 4 1\nMASK -1 -1 1\nCANE 2 4 0\nSWORD 3 4 1\n6\nSWORD 0\nMASK 0\nSCROLL 0\nSWORD 1\nSHIELD 1\nMASK 1");
        }
    }
}
