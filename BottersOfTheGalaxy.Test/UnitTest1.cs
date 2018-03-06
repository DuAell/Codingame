using System;
using NUnit.Framework;

namespace BottersOfTheGalaxy.Test
{
    [TestFixture]
    public class UnitTest1
    {
        [TestCase("0\n0\n23\nBronze_Gadget_3 147 0 214 214 0 0 0 0 0\nBronze_Gadget_5 143 0 158 158 70 70 0 0 0\nSilver_Blade_10 368 38 100 100 100 100 0 0 0\nmana_potion 25 0 0 0 50 0 0 0 1\nBronze_Blade_4 58 8 0 0 0 0 0 0 0\nxxl_potion 330 0 500 0 0 0 0 0 1\nBronze_Blade_2 76 6 0 0 66 66 0 0 0\nGolden_Gadget_13 990 0 0 0 100 100 0 24 0\nLegendary_Blade_16 1493 224 900 900 0 0 150 0 0\nLegendary_Blade_17 1459 123 2307 2307 0 0 0 0 0\nBronze_Blade_1 168 24 0 0 0 0 0 0 0\nLegendary_Blade_19 1373 220 0 0 0 0 150 0 0\nSilver_Blade_7 264 17 148 148 100 100 0 0 0\nSilver_Blade_9 363 41 61 61 0 0 0 1 0\nLegendary_Blade_20 1499 402 0 0 0 0 0 0 0\nSilver_Boots_8 395 0 0 0 0 0 118 0 0\nGolden_Boots_14 946 0 0 0 100 100 21 21 0\nSilver_Boots_6 453 0 0 0 0 0 137 0 0\nGolden_Boots_15 681 0 732 732 0 0 75 0 0\nlarger_potion 70 0 100 0 0 0 0 0 1\nGolden_Blade_12 825 12 0 0 100 100 0 17 0\nGolden_Blade_11 716 39 0 0 0 0 0 11 0\nLegendary_Boots_18 1346 0 0 0 100 100 10 39 0\n15\n394\n1\n10\n1 0 TOWER 100 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n2 1 TOWER 1820 540 400 1500 1500 0 100 0 0 0 0 0 0 0 0 0 - 1 0\n3 0 HERO 735 570 270 216 881 0 109 200 0 300 0 0 0 200 200 3 IRONMAN 1 2\n4 1 HERO 1212 541 245 675 955 0 82 200 0 300 0 0 0 300 300 2 DOCTOR_STRANGE 1 4\n20 1 UNIT 1067 540 300 145 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n28 1 UNIT 1063 590 300 36 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n36 1 UNIT 1063 490 300 71 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n41 1 UNIT 855 522 90 240 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n43 1 UNIT 857 547 90 37 400 0 25 150 0 30 0 0 0 0 0 0 - 1 0\n44 1 UNIT 1067 540 300 250 250 0 35 150 0 50 0 0 0 0 0 0 - 1 0\n")]
        public void RunGame(string data)
        {
            var game = new Player();
            game.DebugMain(data);
        }
    }
}
