﻿using System;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRoyale.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string input = "24\n0 1478 387 83\n1 442 613 83\n2 1073 632 60\n3 847 368 60\n4 983 174 84\n5 937 826 84\n6 1498 829 81\n7 422 171 81\n8 1311 178 88\n9 609 822 88\n10 715 172 82\n11 1205 828 82\n12 307 389 75\n13 1613 611 75\n14 1140 405 84\n15 780 595 84\n16 1765 799 65\n17 155 201 65\n18 1746 380 84\n19 174 620 84\n20 1604 168 78\n21 316 832 78\n22 573 367 66\n23 1347 633 66\n40 3\n0 -1 -1 -1 -1 -1 -1\n1 -1 -1 -1 -1 -1 -1\n2 -1 -1 -1 -1 -1 -1\n3 314 4 -1 -1 -1 -1\n4 295 4 -1 -1 -1 -1\n5 -1 -1 -1 -1 -1 -1\n6 -1 -1 0 1 -1 -1\n7 199 2 0 0 2 -1\n8 -1 -1 -1 -1 -1 -1\n9 -1 -1 -1 -1 -1 -1\n10 267 3 -1 -1 -1 -1\n11 -1 -1 -1 -1 -1 -1\n12 183 3 0 0 3 -1\n13 -1 -1 2 1 0 0\n14 -1 -1 -1 -1 -1 -1\n15 257 4 -1 -1 -1 -1\n16 -1 -1 0 1 -1 -1\n17 211 1 0 0 1 -1\n18 -1 -1 1 1 664 467\n19 -1 -1 -1 -1 -1 -1\n20 -1 -1 -1 -1 -1 -1\n21 -1 -1 -1 -1 -1 -1\n22 274 3 2 0 2 0\n23 -1 -1 -1 -1 -1 -1\n10\n830 289 0 0 27\n925 468 0 0 27\n782 416 0 0 27\n874 468 0 0 27\n757 353 0 -1 95\n907 422 1 0 21\n949 435 1 0 21\n809 448 1 0 21\n850 449 1 0 21\n1583 432 1 -1 95\n";

            new Game().LaunchDebug(input);
        }
    }
}
