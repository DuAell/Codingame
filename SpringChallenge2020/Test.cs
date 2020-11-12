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
            var initData = "33 15§#################################§###     # # #   #   # # #     ###§##### # # # # ##### # # # # #####§# #   #     #   #   #     #   # #§# # # # ### # # # # # ### # # # #§#     #   #   #   #   #   #     #§### # ### # # ##### # # ### # ###§    #       #       #       #    §### ### ### ### # ### ### ### ###§          #     #     #          §### # # # # # # # # # # # # # ###§#     #   # #       # #   #     #§# # ### # # ### # ### # # ### # #§# #     #   #   #   #   #     # #§#################################";
            var turnDatas = new Queue<string>();
            turnDatas.Enqueue("47 38§3§1 1 0 7 PAPER 0 2§1 0 3 7 PAPER 0 0§2 1 14 7 SCISSORS 0 2§3§1 7 1§2 7 1§13 7 1");
            //turnDatas.Enqueue("90 91§4§0 1 17 8 SCISSORS 0 0§1 1 16 9 PAPER 0 0§2 1 17 11 PAPER 0 0§3 1 17 10 ROCK 0 0§0");
            Player.Game.Play(initData, turnDatas);
        }
    }
}
