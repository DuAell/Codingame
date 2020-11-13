using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace FallChallenge2020
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestMethod1()
        {
            const string input = "13§47 BREW -3 -2 0 0 9 -1 -1 0 0§63 BREW 0 -3 0 -2 17 -1 -1 0 0§67 BREW 0 -1 -2 -1 12 -1 -1 0 0§77 BREW -1 -1 -1 -3 20 -1 -1 0 0§55 BREW 0 -2 -3 0 12 -1 -1 0 0§78 CAST 2 0 0 0 0 -1 -1 1 0§79 CAST -1 1 0 0 0 -1 -1 1 0§80 CAST 0 -1 1 0 0 -1 -1 1 0§81 CAST 0 0 -1 1 0 -1 -1 1 0§82 OPPONENT_CAST 2 0 0 0 0 -1 -1 1 0§83 OPPONENT_CAST -1 1 0 0 0 -1 -1 1 0§84 OPPONENT_CAST 0 -1 1 0 0 -1 -1 1 0§85 OPPONENT_CAST 0 0 -1 1 0 -1 -1 1 0§3 0 0 0 0§3 0 0 0 0§13§47 BREW -3 -2 0 0 9 -1 -1 0 0§63 BREW 0 -3 0 -2 17 -1 -1 0 0§67 BREW 0 -1 -2 -1 12 -1 -1 0 0§77 BREW -1 -1 -1 -3 20 -1 -1 0 0§55 BREW 0 -2 -3 0 12 -1 -1 0 0§78 CAST 2 0 0 0 0 -1 -1 0 0§79 CAST -1 1 0 0 0 -1 -1 1 0§80 CAST 0 -1 1 0 0 -1 -1 1 0§81 CAST 0 0 -1 1 0 -1 -1 1 0§82 OPPONENT_CAST 2 0 0 0 0 -1 -1 1 0§83 OPPONENT_CAST -1 1 0 0 0 -1 -1 0 0§84 OPPONENT_CAST 0 -1 1 0 0 -1 -1 1 0§85 OPPONENT_CAST 0 0 -1 1 0 -1 -1 1 0§5 0 0 0 0§2 1 0 0 0";

            new Game().Launch(input);
        }
    }
}
