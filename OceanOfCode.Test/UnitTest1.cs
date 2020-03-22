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
                    "15 15 0§.....xx........§...............§...............§...xxx.........§...xxx.........§...xxx.........§.xx.xx.........§.xx............§....xxx........§....xxx........§.....xx........§.....xx...xx...§..........xx...§xx..xx.........§xx..xx.........";
                var turnData =
                    "10 14 6 6 3 -1 -1 -1§NA§NA§10 13 6 6 2 -1 -1 -1§NA§MOVE W§11 13 6 6 1 -1 -1 -1§NA§MOVE N§12 13 6 6 0 -1 -1 -1§NA§MOVE W§12 12 5 6 3 -1 -1 -1§NA§MOVE W§12 11 5 6 2 -1 -1 -1§NA§TORPEDO 2 0|MOVE W§12 10 5 6 1 -1 -1 -1§NA§MOVE W§12 9 5 6 0 -1 -1 -1§NA§MOVE N§12 8 5 5 3 -1 -1 -1§NA§TORPEDO 2 0|MOVE N§12 7 5 5 2 -1 -1 -1§NA§MOVE E§12 6 5 5 1 -1 -1 -1§NA§MOVE E§12 5 5 5 0 -1 -1 -1§NA§MOVE S";
                var startPositionData = "10 4";
                Game.Play(initData, turnData, startPositionData);
            }
            catch (Exception ex)
            {

            }
        }
    }
}