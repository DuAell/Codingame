using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class Program
{
    static void Main(string[] args)
    {
        var game = new Game();
        game.Launch();
    }
}

public class Game
{
    public InputProcessor InputProcessor { get; private set; }

    public List<Order> Orders { get; set; }
    public List<Spell> Spells { get; set; }
    public List<Player> Players { get; set; }
    public Player Me => Players[0];
    public Player Opponent => Players[1];

    public void Launch()
    {
        // game loop
        while (true)
        {
            LaunchTurn();
        }
    }

    public void LaunchTurn(string debugData = null)
    {
        InputProcessor = new InputProcessor(debugData);

        string[] inputs;
        var actionCount = int.Parse(InputProcessor.ReadLine()); // the number of spells and recipes in play
        Orders = new List<Order>();
        Spells = new List<Spell>();
        for (var i = 0; i < actionCount; i++)
        {
            inputs = InputProcessor.ReadLine().Split(' ');

            switch (inputs[1])
            {
                case "BREW":
                    var order = new Order();
                    order.Create(inputs);
                    Orders.Add(order);
                    break;
                case "CAST":
                    var spell = new Spell();
                    spell.Create(inputs);
                    Spells.Add(spell);
                    break;
            }
        }

        Players = new List<Player>();
        for (var i = 0; i < 2; i++)
        {
            inputs = InputProcessor.ReadLine().Split(' ');
            var player = new Player
            {
                Score = int.Parse(inputs[4])// amount of rupees;
            };
            Players.Add(player);
            player.Inventory.Inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
            player.Inventory.Inv1 = int.Parse(inputs[1]);
            player.Inventory.Inv2 = int.Parse(inputs[2]);
            player.Inventory.Inv3 = int.Parse(inputs[3]);
        }

        InputProcessor.WriteDebugData();

        foreach (var order in Orders.OrderByDescending(_ => _.Price))
        {
            if (order.CanUse(Me.Inventory))
            {
                Console.WriteLine($"BREW {order.Id}");
                return;
            }
        }

        foreach (var spell in Spells.Where(_ => _.Castable && _.CanUse(Me.Inventory)))
        {
            var inventory = Me.Inventory.Clone();
            spell.Use(inventory);
            if (Orders.Any(_ => _.CanUse(inventory)))
            {
                Console.WriteLine($"CAST {spell.Id} Will brew");
                return;
            }
        }

        foreach (var spell in Spells.Where(_ => _.Castable && _.CanUse(Me.Inventory)))
        {
            Console.WriteLine($"CAST {spell.Id} Casting for fun");
            return;
        }
        
        // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
        Console.WriteLine("REST");
    }
}

public abstract class AbstractAction
{
    public int Id { get; set; }
    public string ActionType { get; set; }
    public int Delta0 { get; set; }
    public int Delta1 { get; set; }
    public int Delta2 { get; set; }
    public int Delta3 { get; set; }
    public int Price { get; set; }
    public int TomeIndex { get; set; }
    public int TaxCount { get; set; }
    public bool Castable { get; set; }
    public bool Repeatable { get; set; }

    public void Create(string[] inputs)
    {
        Id = int.Parse(inputs[0]); // the unique ID of this spell or recipe
        ActionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
        Delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
        Delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
        Delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
        Delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
        Price = int.Parse(inputs[6]); // the price in rupees if this is a potion
        TomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax
        TaxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell
        Castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
        Repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell
    }


}

public class DefaultAction : AbstractAction
{
    
}

public class Order : AbstractAction
{
    public bool CanUse(Inventory inventory)
    {
        return inventory.Inv0 + Delta0 >= 0 &&
               inventory.Inv1 + Delta1 >= 0 &&
               inventory.Inv2 + Delta2 >= 0 &&
               inventory.Inv3 + Delta3 >= 0;
    }
}

public class Spell : AbstractAction
{
    public bool CanUse(Inventory inventory)
    {
        var inventoryCloned = inventory.Clone();
        Use(inventoryCloned);
        return inventoryCloned.Size <= 10;
    }

    public void Use(Inventory inventory)
    {
        inventory.Inv0 += Delta0;
        inventory.Inv1 += Delta1;
        inventory.Inv2 += Delta2;
        inventory.Inv3 += Delta3;
    }
}

public class Player
{
    public Inventory Inventory { get; set; } = new Inventory();

    public int Score { get; set; }
}

public class Inventory
{
    public int Inv0 { get; set; }
    public int Inv1 { get; set; }
    public int Inv2 { get; set; }
    public int Inv3 { get; set; }

    public int Size => Inv0 + Inv1 + Inv2 + Inv3;

    public Inventory Clone()
    {
        return (Inventory) MemberwiseClone();
    }
}

public class InputProcessor
{
    public bool IsDebug { get; }
    private readonly Queue<string> _debugInputLines = new Queue<string>();
    private readonly List<string> _readLines = new List<string>();

    public InputProcessor(string debugData = null)
    {
        if (string.IsNullOrEmpty(debugData))
            return;

        IsDebug = true;

        foreach (var s in debugData.Split('§'))
        {
            _debugInputLines.Enqueue(s);
        }
    }

    public string ReadLine()
    {
        var line = IsDebug ? _debugInputLines.Dequeue() : Console.ReadLine();

        _readLines.Add(line);

        return line;
    }

    public void WriteDebugData()
    {
        _readLines.ForEach(Console.Error.WriteLine);
        //Console.Error.WriteLine(string.Join("§", _readLines));
    }
}