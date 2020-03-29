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
                    "15 15 0§xxxx..xx..xxxxx§xxxx..xx..xxxxx§..xx......xxxxx§..........xx...§...............§...............§..........xx..x§..........xx..x§....xx.........§....xx.........§............xx.§............xx.§..xx...........§..xx....xx.....§........xx.....";
                var turnData =
                    "4 14 6 6 3 4 6 -1§NA§NA§4 13 6 5 2 4 6 -1§NA§SURFACE 7";
                var startPositionData = "10 4";
                Game.Play(initData, turnData, startPositionData);
            }
            catch (Exception ex)
            {

            }
        }
    }
}