using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int myTeam = int.Parse(Console.ReadLine());
        int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }
        int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
        for (int i = 0; i < itemCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
            int itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
            int damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
            int health = int.Parse(inputs[3]);
            int maxHealth = int.Parse(inputs[4]);
            int mana = int.Parse(inputs[5]);
            int maxMana = int.Parse(inputs[6]);
            int moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
            int manaRegeneration = int.Parse(inputs[8]);
            int isPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed
        }

        // game loop
        while (true)
        {
            List<Unit> _units = new List<Unit>();

            int gold = int.Parse(Console.ReadLine());
            int enemyGold = int.Parse(Console.ReadLine());
            int roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command
            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                switch (inputs[2])
                {
                    case "UNIT":
                        _units.Add(new Unit(inputs));
                        break;
                    case "HERO":
                        _units.Add(new Hero(inputs));
                        break;
                    case "TOWER":
                        _units.Add(new Tower(inputs));
                        break;
                    case "GROOT":
                        _units.Add(new Groot(inputs));
                        break;
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
            // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
            if (roundType < 0)
            {
                Console.WriteLine("HULK");
            }
            else
            {
                Console.WriteLine("ATTACK_NEAREST UNIT");
            }
        }
    }

    public class Position
    {
        public double X;
        public double Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Unit : Position
    {
        public int unitId;
        public int team;
        public int attackRange;
        public int health;
        public int maxHealth;
        public int shield;
        public int attackDamage;
        public int movementSpeed;
        public int stunDuration;
        public int goldValue;
        
        public Unit(string[] inputs) : base(int.Parse(inputs[3]), int.Parse(inputs[4]))
        {
            unitId = int.Parse(inputs[0]);
            team = int.Parse(inputs[1]);
            attackRange = int.Parse(inputs[5]);
            health = int.Parse(inputs[6]);
            maxHealth = int.Parse(inputs[7]);
            shield = int.Parse(inputs[8]); // useful in bronze
            attackDamage = int.Parse(inputs[9]);
            movementSpeed = int.Parse(inputs[10]);
            stunDuration = int.Parse(inputs[11]); // useful in bronze
            goldValue = int.Parse(inputs[12]);
        }
    }

    public class Hero : Unit
    {
        public int countDown1;
        public int countDown2;
        public int countDown3;
        public int mana;
        public int maxMana;
        public int manaRegeneration;
        public string heroType;
        public bool isVisible;
        public int itemsOwned;

        public Hero(string[] inputs) : base(inputs)
        {
            countDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
            countDown2 = int.Parse(inputs[14]);
            countDown3 = int.Parse(inputs[15]);
            mana = int.Parse(inputs[16]);
            maxMana = int.Parse(inputs[17]);
            manaRegeneration = int.Parse(inputs[18]);
            heroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
            isVisible = int.Parse(inputs[20]) == 1; // 0 if it isn't
            itemsOwned = int.Parse(inputs[21]); // useful from wood1
        }
    }

    public class Tower : Unit
    {
        public Tower(string[] inputs) : base(inputs)
        {

        }
    }

    public class Groot : Unit
    {
        public Groot(string[] inputs) : base(inputs)
        {

        }
    }
}