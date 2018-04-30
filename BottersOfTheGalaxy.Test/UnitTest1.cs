﻿using System;
using NUnit.Framework;

namespace BottersOfTheGalaxy.Test
{
    [TestFixture]
    public class UnitTest1
    {
        //[TestCase("0\n19\nBUSH 682 720 50\nBUSH 1238 720 50\nBUSH 238 380 50\nBUSH 1682 380 50\nBUSH 787 380 50\nBUSH 1133 380 50\nBUSH 88 330 50\nBUSH 1832 330 50\nBUSH 856 76 50\nBUSH 1064 76 50\nBUSH 1740 123 50\nBUSH 180 123 50\nBUSH 1018 273 50\nBUSH 902 273 50\nBUSH 756 178 50\nBUSH 1164 178 50\nSPAWN 94 266 0\nSPAWN 960 199 0\nSPAWN 1826 266 0\n23\nBronze_Gadget_4 147 0 0 0 0 0 0 3 0\nGolden_Blade_14 569 6 847 847 0 0 0 0 0\nSilver_Blade_10 468 71 0 0 0 0 0 0 0\nLegendary_Gadget_16 1496 0 2124 2124 0 0 0 27 0\nmana_potion 25 0 0 0 50 0 0 0 1\nBronze_Gadget_1 34 0 0 0 68 68 0 0 0\nLegendary_Gadget_19 1122 0 1204 1204 0 0 0 13 0\nxxl_potion 330 0 500 0 0 0 0 0 1\nBronze_Blade_2 120 10 0 0 100 100 0 0 0\nBronze_Boots_3 120 9 0 0 0 0 16 0 0\nLegendary_Blade_18 1264 8 2500 2500 0 0 0 0 0\nBronze_Boots_5 82 0 0 0 0 0 9 1 0\nSilver_Blade_8 229 33 0 0 0 0 0 0 0\nSilver_Blade_9 175 18 0 0 0 0 0 1 0\nSilver_Gadget_6 450 0 57 57 100 100 0 8 0\nGolden_Boots_11 843 42 174 174 100 100 150 0 0\nGolden_Gadget_15 834 0 0 0 100 100 0 19 0\nLegendary_Boots_20 1330 87 244 244 100 100 150 12 0\nlarger_potion 70 0 100 0 0 0 0 0 1\nSilver_Boots_7 415 0 180 180 0 0 20 5 0\nGolden_Blade_13 877 148 0 0 0 0 0 0 0\nGolden_Blade_12 903 6 1020 1020 100 100 0 6 0\nLegendary_Boots_17 1045 0 0 0 100 100 138 16 0\n252\n252\n2\n14\n1 0 TOWER 100 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n2 1 TOWER 1820 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n3 0 HERO 200 590 110 1380 1380 0 80 200 0 300 0 0 0 100 100 1 DEADPOOL 1 0\n4 1 HERO 1720 590 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0\n5 0 HERO 200 490 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0\n6 1 HERO 1720 490 270 820 820 0 60 200 0 300 0 0 0 200 200 2 IRONMAN 1 0\n7 0 UNIT 160 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n8 0 UNIT 160 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n9 0 UNIT 160 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n10 0 UNIT 110 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n11 1 UNIT 1760 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n12 1 UNIT 1760 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n13 1 UNIT 1760 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n14 1 UNIT 1810 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n")]
        [TestCase("1\n19\nBUSH 318 720 50\nBUSH 1602 720 50\nBUSH 517 720 50\nBUSH 1403 720 50\nBUSH 679 720 50\nBUSH 1241 720 50\nBUSH 329 380 50\nBUSH 1591 380 50\nBUSH 854 380 50\nBUSH 1066 380 50\nBUSH 467 127 50\nBUSH 1453 127 50\nBUSH 1106 181 50\nBUSH 814 181 50\nBUSH 1329 113 50\nBUSH 591 113 50\nSPAWN 601 156 0\nSPAWN 960 154 0\nSPAWN 1319 156 0\n23\nGolden_Blade_15 664 82 0 0 100 100 41 0 0\nLegendary_Gadget_16 1200 0 0 0 0 0 0 43 0\nmana_potion 25 0 0 0 50 0 0 0 1\nBronze_Blade_4 146 7 0 0 100 100 0 1 0\nBronze_Blade_5 142 5 157 157 0 0 0 0 0\nxxl_potion 326 0 500 0 0 0 0 0 1\nBronze_Blade_2 160 10 62 62 0 0 0 1 0\nGolden_Gadget_13 562 0 920 920 0 0 0 0 0\nBronze_Boots_3 147 5 0 0 100 100 18 0 0\nLegendary_Blade_17 1200 88 2208 2208 100 100 0 0 0\nLegendary_Blade_19 1200 134 420 420 100 100 0 16 0\nSilver_Gadget_8 200 0 298 298 0 0 0 0 0\nSilver_Gadget_6 365 0 137 137 0 0 0 6 0\nSilver_Boots_10 277 0 0 0 0 0 26 4 0\nGolden_Boots_14 729 48 0 0 0 0 150 0 0\nBronze_Boots_1 99 0 0 0 0 0 14 1 0\nlarger_potion 70 0 100 0 0 0 0 0 1\nSilver_Boots_7 225 0 50 50 100 100 14 2 0\nLegendary_Gadget_20 1166 0 0 0 0 0 0 37 0\nGolden_Blade_12 573 37 61 61 100 100 0 6 0\nGolden_Blade_11 921 96 750 750 0 0 0 0 0\nSilver_Gadget_9 192 0 0 0 100 100 0 3 0\nLegendary_Boots_18 1200 36 2500 2500 100 100 150 0 0\n622\n622\n2\n14\n1 0 TOWER 100 540 400 3000 3000 0 190 0 0 0 0 0 0 0 0 0 - 1 0\n2 1 TOWER 1820 540 400 3000 3000 0 190 0 0 0 0 0 0 0 0 0 - 1 0\n3 0 HERO 200 590 95 1450 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0\n4 1 HERO 1720 590 270 820 820 0 60 200 0 300 0 0 0 200 200 2 IRONMAN 1 0\n5 0 HERO 200 490 270 820 820 0 60 200 0 300 0 0 0 200 200 2 IRONMAN 1 0\n6 1 HERO 1720 490 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0\n7 0 UNIT 160 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n8 0 UNIT 160 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n9 0 UNIT 160 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n10 0 UNIT 110 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n11 1 UNIT 1760 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n12 1 UNIT 1760 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n13 1 UNIT 1760 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n14 1 UNIT 1810 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n")]
        public void RunGame(string data)
        {
            var game = new Player();
            game.DebugMain(data);
        }
    }
}
