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
                    "15 15 0§....xxxx.......§....xxxx.......§....xxx....xx..§...........xx..§........xx.....§xx......xx.....§xx..........xxx§............xxx§............xxx§...........xxxx§...........xxxx§...............§...............§...............§...............";
                var turnData =
                    "8 3 6 6 3 4 6 -1§NA§NA§8 2 6 5 2 4 6 -1§NA§SURFACE 7§8 1 6 5 1 4 6 -1§NA§MOVE E§8 0 6 5 0 4 6 -1§NA§MOVE N§9 0 6 5 0 3 6 -1§NA§MOVE N§9 1 6 5 0 2 6 -1§NA§MOVE N§9 2 6 5 0 1 6 -1§NA§MOVE N§9 3 6 5 0 0 6 -1§NA§MOVE E§9 4 6 5 0 4 6 -1§N§MOVE E§9 5 6 5 0 3 6 -1§NA§MOVE E";
                var startPositionData = "10 4";
                Game.Play(initData, turnData, startPositionData);
            }
            catch (Exception ex)
            {

            }
        }
    }
}