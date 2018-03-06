using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
public class Player
{
    private static string debugInitInput;
    private static string debugInput;
    private static int myTeam;
    private static Hero MyHero;
    private static List<Bush> _bushes = new List<Bush>();
    private static List<Item> _items = new List<Item>();
    private static List<Unit> _units;
    private static int gold;
    private static int enemyGold;
    private static int roundType; // a positive value will show the number of heroes that await a command

    static string ReadLine(bool isInit = false)
    {
        string input = Console.ReadLine();
        if (isInit)
            debugInitInput += input + @"\n";
        else
            debugInput += input + @"\n";
        return input;
    }

    public void DebugMain(string data)
    {
        using (var sr = new StringReader(data))
        {
            string[] inputs;
            myTeam = int.Parse(sr.ReadLine());

            int bushAndSpawnPointCount = int.Parse(sr.ReadLine()); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
            for (int i = 0; i < bushAndSpawnPointCount; i++)
            {
                inputs = sr.ReadLine().Split(' ');
                _bushes.Add(new Bush(inputs));
            }

            int itemCount = int.Parse(sr.ReadLine()); // useful from wood2
            for (int i = 0; i < itemCount; i++)
            {
                inputs = sr.ReadLine().Split(' ');
                _items.Add(new Item(inputs));
            }

            _units = new List<Unit>();

            // game loop
            //while (true)
            //{
            
            gold = int.Parse(sr.ReadLine());
            enemyGold = int.Parse(sr.ReadLine());
            roundType = int.Parse(sr.ReadLine()); // a positive value will show the number of heroes that await a command

            _units = new List<Unit>();
            int entityCount = int.Parse(sr.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = sr.ReadLine().Split(' ');
                ReadUnits(inputs);
            }

            GameLogic();
            //}
        }
    }

    public static void ReadUnits(string[] inputs)
    {
        Unit unit = null;

        switch (inputs[2])
        {
            case "UNIT":
                unit = new Unit(inputs);
                break;
            case "HERO":
                unit = new Hero(inputs);
                if (unit.team == myTeam)
                    MyHero = (Hero)unit;
                break;
            case "TOWER":
                unit = new Tower(inputs);
                break;
            case "GROOT":
                unit = new Groot(inputs);
                break;
        }

        if (unit.team == myTeam)
            unit.IsMine = true;
        _units.Add(unit);
    }

    public static void Main(string[] args)
    {
        string[] inputs;
        myTeam = int.Parse(ReadLine(true));

        int bushAndSpawnPointCount = int.Parse(ReadLine(true)); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            inputs = ReadLine(true).Split(' ');
            _bushes.Add(new Bush(inputs));
        }

        int itemCount = int.Parse(ReadLine(true)); // useful from wood2
        for (int i = 0; i < itemCount; i++)
        {
            inputs = ReadLine(true).Split(' ');
            _items.Add(new Item(inputs));
        }

        // game loop
        while (true)
        {
            debugInput = string.Empty;

            gold = int.Parse(ReadLine());
            enemyGold = int.Parse(ReadLine());
            roundType = int.Parse(ReadLine()); // a positive value will show the number of heroes that await a command

            _units = new List<Unit>();
            int entityCount = int.Parse(ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = ReadLine().Split(' ');
                ReadUnits(inputs);
            }

            Console.Error.WriteLine(debugInitInput + debugInput);

            new Player().GameLogic();
        }
    }

    public void GameLogic()
    {
        // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
        // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
        if (roundType < 0)
        {
            if (MyHero == null)
                Console.WriteLine("IRONMAN");
            else
                Console.WriteLine("VALKYRIE");
            return;
        }

        Console.WriteLine("ATTACK_NEAREST UNIT");

        Item affordableItem;
        // Buy health potion            
        if (MyHero.health / MyHero.maxHealth < 0.7)
        {
            affordableItem = _items.OrderByDescending(x => x.health).Where(x => x.isPotion && x.health > 0 && x.itemCost <= gold).FirstOrDefault();
            if (affordableItem != null)
            {
                Console.WriteLine($"BUY {affordableItem.itemName}");
                return;
            }
        }

        // Buying stuff
        affordableItem = _items.OrderByDescending(x => x.damage).Where(x => x.itemCost <= gold).FirstOrDefault();
        if (MyHero.itemsOwned < 3 && affordableItem != null)
        {
            Console.WriteLine($"BUY {affordableItem.itemName}");
            return;
        }

        var nearestUnits = _units.Where(x => !x.IsMine).OrderBy(x => MyHero.Distance(x));
        Console.Error.WriteLine($"MyHero pos : {MyHero.X} {MyHero.Y}");
        foreach (var unit in nearestUnits)
        {
            Console.Error.WriteLine($"Nearest unit pos : {unit.X} {unit.Y}");
            Console.Error.WriteLine($"Nearest unit movement speed : {unit.movementSpeed}");
            Console.Error.WriteLine($"Nearest unit attack range : {unit.attackRange}");
            Console.Error.WriteLine($"Distance : {MyHero.Distance(unit)}");
            Console.Error.WriteLine($"IsAtAttackDistance : {unit.IsAtAttackDistance(MyHero)}");
        }

        var nearestUnit = nearestUnits.FirstOrDefault(x => x.IsAtAttackDistance(MyHero));
        if (nearestUnit != null)
        {
            Console.Error.WriteLine($"MyHero pos : {MyHero.X} {MyHero.Y}");
            Console.Error.WriteLine($"Nearest unit pos : {nearestUnit.X} {nearestUnit.Y}");
            Console.Error.WriteLine($"Nearest unit movement speed : {nearestUnit.movementSpeed}");
            Console.Error.WriteLine($"Nearest unit attack range : {nearestUnit.attackRange}");
            Console.Error.WriteLine($"Distance : {MyHero.Distance(nearestUnit)}");
            Console.Error.WriteLine($"IsAtAttackDistance : {nearestUnit.IsAtAttackDistance(MyHero)}");
            if (nearestUnit.X > MyHero.X)
                Console.WriteLine("MOVE 0 590");
            else
                Console.WriteLine("MOVE 1920 590");
            return;
        }

        // Attack 
        var readyToDieUnit = _units.Where(x => MyHero.IsAtAttackDistance(x) && x.health < MyHero.attackDamage && (!x.IsMine || x.GetType() == typeof(Unit))).FirstOrDefault();
        if (readyToDieUnit != null)
        {
            Console.WriteLine($"ATTACK {readyToDieUnit.unitId}");
            return;
        }

        Console.WriteLine("ATTACK_NEAREST UNIT");
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

    static double Sqr(double a)
    {
        return a * a;
    }

    public int Distance(Unit unit)
    {
        return (int)Math.Sqrt(Sqr(unit.Y - Y) + Sqr(unit.X - X));
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

    public bool IsMine;

    public bool IsAtAttackDistance(Unit unit)
    {
        return attackRange >= Distance(unit);
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

public class Item
{
    public string itemName;
    public int itemCost;
    public int damage;
    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    public int moveSpeed;
    public int manaRegeneration;
    public bool isPotion;

    public Item(string[] inputs)
    {
        itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
        itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
        damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
        health = int.Parse(inputs[3]);
        maxHealth = int.Parse(inputs[4]);
        mana = int.Parse(inputs[5]);
        maxMana = int.Parse(inputs[6]);
        moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
        manaRegeneration = int.Parse(inputs[8]);
        isPotion = int.Parse(inputs[9]) == 1; // 0 if it's not instantly consumed
    }
}

public class Bush : Position
{
    public string entityType;
    public int radius;

    public Bush(string[] inputs) : base(int.Parse(inputs[1]), int.Parse(inputs[2]))
    {
        entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
        radius = int.Parse(inputs[3]);
    }
}