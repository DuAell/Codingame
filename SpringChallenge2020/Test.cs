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
            var initData = "31 13§###############################§    # #   #   ###   #   # #    §### # ### # ####### # ### # ###§#     #   #   # #   #   #     #§# ### # ### # # # # ### # ### #§# #         #     #         # #§# # ##### # ### ### # ##### # #§#       # #         # #       #§### # # # ### ### ### # # # ###§### # #                 # # ###§### # ##### ### ### ##### # ###§    #       #     #       #    §###############################";
            var turnDatas = new Queue<string>();
            turnDatas.Enqueue("49 47§3§0 1 8 3 SCISSORS 0 3§1 0 1 1 PAPER 0 3§1 1 30 1 PAPER 0 3§1§0 1 1");
            //turnDatas.Enqueue("90 91§4§0 1 17 8 SCISSORS 0 0§1 1 16 9 PAPER 0 0§2 1 17 11 PAPER 0 0§3 1 17 10 ROCK 0 0§0");
            Player.Game.Play(initData, turnDatas);
        }
    }
}
