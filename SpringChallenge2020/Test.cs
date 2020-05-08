using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SpringChallenge2020
{
    [TestFixture]
    class Test
    {
        [Test]
        public void TestTurn()
        {
            var initData = "35 12§###################################§### # # # ###   # #   ### # # # ###§### # # # ### ### ### ### # # # ###§        #                 #        §##### ### # # ####### # # ### #####§          #     # #     #          §### # ### ##### # # ##### ### # ###§    # ###                 ### #    §### # ### ##### ### ##### ### # ###§#   #     #     ###     #     #   #§# ### # # # # ####### # # # # ### #§###################################";
            var turnData = "4 4§10§0 1 6 9 ROCK 0 0§0 0 28 9 ROCK 0 0§1 1 27 9 PAPER 0 0§1 0 7 9 PAPER 0 0§2 1 25 2 SCISSORS 0 0§2 0 9 2 SCISSORS 0 0§3 1 25 3 ROCK 0 0§3 0 9 3 ROCK 0 0§4 1 9 4 PAPER 0 0§4 0 25 6 PAPER 0 0§29§8 9 1§9 9 1§26 9 1§25 9 1§25 4 1§25 7 1§25 8 1§25 10 1§24 3 1§22 3 1§21 3 1§20 3 1§19 3 1§18 3 1§17 3 1§16 3 1§15 3 1§14 3 1§13 3 1§12 3 1§10 3 1§9 6 1§9 7 1§9 8 1§9 10 1§11 3 10§23 3 10§11 7 10§23 7 10";
            Player.Game.Play(initData, turnData);
        }
    }
}
