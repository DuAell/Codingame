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
            var initData = "35 17§###################################§#       #   #         #   #       #§# ### ### ### ####### ### ### ### #§# #                             # #§# # ##### # # ####### # # ##### # #§#       #   #         #   #       #§### # # # ##### ### ##### # # # ###§    # #                     # #    §### # ### ### # ### # ### ### # ###§### #         #     #         # ###§### ### # ##### ### ##### # ### ###§        #                 #        §### # # ### # # ### # # ### # # ###§#     #       #     #       #     #§# # ##### ### # ### # ### ##### # #§# #       #   # ### #   #       # #§###################################";
            var turnDatas = new Queue<string>();
            turnDatas.Enqueue("49 80§5§0 1 21 2 SCISSORS 0 0§1 1 29 2 PAPER 0 0§2 0 17 1 SCISSORS 0 0§2 1 21 1 SCISSORS 0 0§3 1 31 10 SCISSORS 0 0§13§29 1 1§20 1 1§19 1 1§18 1 1§31 6 1§31 5 1§31 4 1§31 3 1§31 11 1§31 12 1§31 13 1§31 14 1§31 15 1");
            //turnDatas.Enqueue("90 91§4§0 1 17 8 SCISSORS 0 0§1 1 16 9 PAPER 0 0§2 1 17 11 PAPER 0 0§3 1 17 10 ROCK 0 0§0");
            Player.Game.Play(initData, turnDatas);
        }
    }
}
