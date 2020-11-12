using System;
using NUnit.Framework;

namespace OceanOfCode.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            try
            {
                var initData =
                    "15 15 0§.xx..xx...xx...§...............§xx.............§xxx............§xxxx...........§xxxx...........§xx.............§xx.............§...............§...............§............xx.§............xx.§...............§...............§...............";
                var turnData =
                    "6 9 6 6 3 4 6 3§NA§NA§5 9 6 6 2 4 6 3§NA§MOVE E§4 9 6 6 1 4 6 3§NA§MOVE E§4 8 6 6 0 4 6 3§NA§MOVE E§3 8 6 6 0 3 6 3§NA§MOVE E§3 7 6 6 0 2 6 3§NA§MOVE E§4 7 6 6 0 1 6 3§NA§MOVE E§5 7 6 6 0 0 6 3§NA§MOVE E§6 7 6 6 0 4 6 3§N§MOVE S§7 7 6 6 0 3 6 3§NA§MOVE S§7 6 6 6 0 2 6 3§NA§SILENCE§7 5 6 6 0 1 6 3§NA§MOVE S§7 4 6 6 0 0 6 3§NA§MOVE S|TORPEDO 14 9§8 4 6 6 0 0 5 3§NA§MOVE S§9 4 6 6 0 0 4 3§NA§MOVE S§10 4 6 6 0 0 3 3§NA§MOVE W§11 4 6 6 0 0 2 3§NA§MOVE N§12 4 6 6 0 0 1 3§NA§MOVE N§12 3 6 6 0 0 0 3§NA§MOVE W§8 3 6 6 0 0 6 3§NA§MOVE S§8 2 6 6 0 0 5 3§NA§SILENCE§9 2 6 6 0 4 5 3§Y§MOVE W§10 2 6 6 0 4 4 3§NA§MOVE N§11 2 6 6 0 4 3 3§NA§MOVE N§12 2 6 6 0 4 2 3§NA§MOVE N§13 2 6 6 0 4 1 3§NA§MOVE N§13 3 6 6 0 4 0 3§NA§MOVE N§13 7 6 6 0 4 6 3§NA§SILENCE§14 7 6 6 0 3 6 3§NA§MOVE E§14 6 6 6 0 2 6 3§NA§MOVE N§14 5 6 6 0 1 6 3§NA§MOVE W§14 4 6 6 0 0 6 3§NA§MOVE W§14 3 6 6 0 4 6 3§N§MOVE W§14 2 6 6 0 3 6 3§NA§MOVE S§14 1 6 6 0 2 6 3§NA§SILENCE§13 1 6 6 0 1 6 3§NA§MOVE S|TORPEDO 10 7§13 0 6 6 0 0 6 3§NA§MOVE S§14 0 6 6 0 4 6 3§Y§MOVE S§14 0 5 6 0 4 6 3§NA§MOVE S§13 0 5 6 0 4 5 3§NA§MOVE W§12 0 5 6 0 4 4 3§NA§MOVE N§12 1 5 6 0 4 3 3§NA§MOVE N|TORPEDO 9 8§11 1 5 6 0 4 2 3§NA§MOVE N§10 1 5 6 0 4 1 3§NA§MOVE N§9 1 5 6 0 4 0 3§NA§MOVE N|TORPEDO 9 7§9 5 5 6 0 4 6 3§NA§MOVE N§10 5 5 5 3 4 5 3§NA§MOVE W§10 6 5 5 2 4 5 3§NA§MOVE S§9 6 5 5 1 4 5 3§NA§MOVE S§8 6 5 5 0 4 5 3§NA§MOVE S§8 7 5 5 3 4 4 3§NA§SILENCE§7 7 5 5 2 4 4 3§NA§MOVE S§7 6 5 5 1 4 4 3§NA§MOVE S§7 5 5 5 0 4 4 3§NA§MOVE W§7 4 5 5 0 3 4 3§NA§MOVE N§7 3 5 5 0 2 4 3§NA§MOVE N|TORPEDO 7 8§7 2 5 5 0 1 4 3§NA§MOVE N§7 1 5 5 0 0 4 3§NA§MOVE N§7 0 5 5 0 4 4 3§Y§MOVE N|TORPEDO 7 7§8 0 5 5 0 4 3 3§NA§MOVE N§8 1 5 5 0 4 2 3§NA§MOVE W§8 2 5 5 0 4 1 3§NA§MOVE S|TORPEDO 6 7§8 3 5 5 0 4 0 3§NA§MOVE S§8 4 5 5 0 4 0 3§NA§MOVE S§8 5 5 5 0 4 0 3§NA§MOVE S|TORPEDO 6 8§8 5 4 5 0 4 0 3§NA§MOVE S§8 1 4 5 0 4 6 3§NA§MOVE S§7 1 4 5 0 4 5 3§NA§MOVE W§6 1 4 5 0 4 4 3§NA§MOVE N§5 1 4 5 0 4 3 3§NA§SILENCE§5 2 4 5 0 4 2 3§NA§MOVE N§5 3 4 5 0 4 1 3§NA§MOVE N§5 4 4 5 0 4 0 3§NA§MOVE N§6 4 4 5 0 4 0 3§NA§MOVE N§6 3 4 5 0 4 0 3§NA§MOVE N§6 2 4 5 0 4 0 3§NA§MOVE E§7 2 4 5 0 4 0 3§NA§SILENCE|TORPEDO 8 7";
                var startPositionData = "6 9";
                Game.Play(initData, turnData, startPositionData);
            }
            catch (Exception ex)
            {

            }
        }
    }
}