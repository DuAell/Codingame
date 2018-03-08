﻿using System;
using NUnit.Framework;

namespace BottersOfTheGalaxy.Test
{
    [TestFixture]
    public class UnitTest1
    {
        //[TestCase("0\n19\nBUSH 682 720 50\nBUSH 1238 720 50\nBUSH 238 380 50\nBUSH 1682 380 50\nBUSH 787 380 50\nBUSH 1133 380 50\nBUSH 88 330 50\nBUSH 1832 330 50\nBUSH 856 76 50\nBUSH 1064 76 50\nBUSH 1740 123 50\nBUSH 180 123 50\nBUSH 1018 273 50\nBUSH 902 273 50\nBUSH 756 178 50\nBUSH 1164 178 50\nSPAWN 94 266 0\nSPAWN 960 199 0\nSPAWN 1826 266 0\n23\nBronze_Gadget_4 147 0 0 0 0 0 0 3 0\nGolden_Blade_14 569 6 847 847 0 0 0 0 0\nSilver_Blade_10 468 71 0 0 0 0 0 0 0\nLegendary_Gadget_16 1496 0 2124 2124 0 0 0 27 0\nmana_potion 25 0 0 0 50 0 0 0 1\nBronze_Gadget_1 34 0 0 0 68 68 0 0 0\nLegendary_Gadget_19 1122 0 1204 1204 0 0 0 13 0\nxxl_potion 330 0 500 0 0 0 0 0 1\nBronze_Blade_2 120 10 0 0 100 100 0 0 0\nBronze_Boots_3 120 9 0 0 0 0 16 0 0\nLegendary_Blade_18 1264 8 2500 2500 0 0 0 0 0\nBronze_Boots_5 82 0 0 0 0 0 9 1 0\nSilver_Blade_8 229 33 0 0 0 0 0 0 0\nSilver_Blade_9 175 18 0 0 0 0 0 1 0\nSilver_Gadget_6 450 0 57 57 100 100 0 8 0\nGolden_Boots_11 843 42 174 174 100 100 150 0 0\nGolden_Gadget_15 834 0 0 0 100 100 0 19 0\nLegendary_Boots_20 1330 87 244 244 100 100 150 12 0\nlarger_potion 70 0 100 0 0 0 0 0 1\nSilver_Boots_7 415 0 180 180 0 0 20 5 0\nGolden_Blade_13 877 148 0 0 0 0 0 0 0\nGolden_Blade_12 903 6 1020 1020 100 100 0 6 0\nLegendary_Boots_17 1045 0 0 0 100 100 138 16 0\n252\n252\n2\n14\n1 0 TOWER 100 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n2 1 TOWER 1820 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n3 0 HERO 200 590 110 1380 1380 0 80 200 0 300 0 0 0 100 100 1 DEADPOOL 1 0\n4 1 HERO 1720 590 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0\n5 0 HERO 200 490 130 1400 1400 0 65 200 0 300 0 0 0 155 155 2 VALKYRIE 1 0\n6 1 HERO 1720 490 270 820 820 0 60 200 0 300 0 0 0 200 200 2 IRONMAN 1 0\n7 0 UNIT 160 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n8 0 UNIT 160 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n9 0 UNIT 160 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n10 0 UNIT 110 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n11 1 UNIT 1760 490 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n12 1 UNIT 1760 540 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n13 1 UNIT 1760 590 90 400 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n14 1 UNIT 1810 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n")]
        [TestCase("0\n26\nBUSH 435 720 50\nBUSH 1485 720 50\nBUSH 654 720 50\nBUSH 1266 720 50\nBUSH 347 380 50\nBUSH 1573 380 50\nBUSH 864 380 50\nBUSH 1056 380 50\nBUSH 153 233 50\nBUSH 1767 233 50\nBUSH 507 197 50\nBUSH 1413 197 50\nBUSH 1370 95 50\nBUSH 550 95 50\nBUSH 1731 346 50\nBUSH 189 346 50\nBUSH 1160 222 50\nBUSH 760 222 50\nBUSH 1152 60 50\nBUSH 768 60 50\nBUSH 1012 186 50\nBUSH 908 186 50\nSPAWN 278 255 0\nSPAWN 502 72 0\nSPAWN 1418 72 0\nSPAWN 1642 255 0\n23\nGolden_Blade_14 884 87 0 0 0 0 0 9 0\nmana_potion 25 0 0 0 50 0 0 0 1\nBronze_Gadget_1 151 0 221 221 0 0 0 0 0\nBronze_Blade_5 107 15 0 0 0 0 0 0 0\nLegendary_Gadget_18 1161 0 461 461 100 100 0 24 0\nxxl_potion 330 0 500 0 0 0 0 0 1\nBronze_Blade_2 72 4 62 62 0 0 0 0 0\nBronze_Boots_4 40 0 0 0 0 0 11 0 0\nBronze_Blade_3 65 9 0 0 0 0 0 0 0\nLegendary_Blade_17 1040 26 0 0 0 0 0 23 0\nGolden_Gadget_11 962 0 217 217 100 100 0 20 0\nSilver_Blade_8 255 22 82 82 0 0 0 1 0\nSilver_Blade_7 281 35 61 61 0 0 0 0 0\nLegendary_Boots_16 1357 0 2117 2117 100 100 150 0 0\nSilver_Gadget_6 426 0 587 587 100 100 0 0 0\nSilver_Boots_10 421 0 77 77 100 100 42 4 0\nGolden_Boots_13 641 4 68 68 0 0 98 6 0\nSilver_Boots_9 364 29 0 0 100 100 36 0 0\nGolden_Gadget_15 650 0 773 773 0 0 0 4 0\nLegendary_Boots_20 1493 0 0 0 0 0 150 45 0\nlarger_potion 70 0 100 0 0 0 0 0 1\nLegendary_Boots_19 1337 72 0 0 0 0 150 19 0\nGolden_Blade_12 887 94 507 507 100 100 0 0 0\n29\n36\n2\n13\n1 0 TOWER 100 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n2 1 TOWER 1820 540 400 3000 3000 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n3 0 HERO 930 551 110 465 1380 0 113 200 0 300 0 0 0 36 100 1 DEADPOOL 1 3\n4 1 HERO 1203 500 95 605 1450 0 80 200 0 300 0 0 0 90 90 1 HULK 1 0\n5 0 HERO 100 540 130 300 1400 0 83 200 0 300 0 0 0 137 155 2 VALKYRIE 1 2\n15 -1 GROOT 278 255 150 400 400 0 35 250 0 100 0 0 0 0 0 0 - 1 0\n16 -1 GROOT 502 72 150 400 400 0 35 250 0 100 0 0 0 0 0 0 - 1 0\n34 1 UNIT 1409 585 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n42 1 UNIT 1409 490 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n44 0 UNIT 1123 541 90 295 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n45 0 UNIT 1131 554 90 85 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n46 0 UNIT 949 541 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n50 1 UNIT 1411 531 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n")]
        public void RunGame(string data)
        {
            var game = new Player();
            game.DebugMain(data);
        }
    }
}