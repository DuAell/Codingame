﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeALaMode.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Program.MainClass.GameData = "20§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES 850§DISH-BLUEBERRIES-ICE_CREAM-CHOPPED_STRAWBERRIES 1050§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES 850§DISH-ICE_CREAM-BLUEBERRIES 650§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES 800§DISH-ICE_CREAM-BLUEBERRIES-CHOPPED_STRAWBERRIES 1050§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES-BLUEBERRIES 1050§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES-BLUEBERRIES 1050§DISH-BLUEBERRIES-ICE_CREAM 650§DISH-ICE_CREAM-BLUEBERRIES 650§DISH-ICE_CREAM-BLUEBERRIES-CHOPPED_STRAWBERRIES 1050§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES 800§DISH-CHOPPED_STRAWBERRIES-BLUEBERRIES-ICE_CREAM 1050§DISH-ICE_CREAM-BLUEBERRIES 650§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES 800§DISH-CHOPPED_STRAWBERRIES-ICE_CREAM 800§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES-BLUEBERRIES 1050§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES-ICE_CREAM 1050§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES-ICE_CREAM 1050§DISH-ICE_CREAM-BLUEBERRIES-CHOPPED_STRAWBERRIES 1050§##B##D#####§#.........#§#.####.##.C§#.#..#..#.#§#.#S.####.#§#.0....1..#§#####W#I###";
            Program.MainClass.TurnData = "75§4 1 DISH§9 1 DISH-BLUEBERRIES§1§8 3 CHOPPED_STRAWBERRIES§NONE 0§3§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES 726§DISH-BLUEBERRIES-CHOPPED_STRAWBERRIES 726§DISH-ICE_CREAM-CHOPPED_STRAWBERRIES 791";
            Program.MainClass.Main();
        }
    }
}