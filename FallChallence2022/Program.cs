﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Position
{
    public int X, Y;

    public string XY => X + " " + Y;
    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int Manhattan(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

    public int Euclidian(Position p2) => (int)Math.Sqrt(Math.Pow(X - p2.X, 2) + Math.Pow(Y - p2.Y, 2));

    public override string ToString()
    {
        return X + " " + Y;
    }

    public Direction? GetDirectionTo(Position destination)
    {
        if (destination.X > X)
            return Direction.East;
        if (destination.X < X)
            return Direction.West;
        if (destination.Y > Y)
            return Direction.South;
        if (destination.Y < Y)
            return Direction.North;
        return null;
    }
}

public enum Direction
{
    North,
    South,
    East,
    West
}

public class Tile
{
    public Position Position { get; set; }

    public Player Player { get; set; }

    public int ScrapAmount { get; set; }

    public bool CanBuild { get; set; }
    public bool CanSpawn { get; set; }
    public bool InRangeOfRecycler { get; set; }
}

public class Map
{
    public List<Tile> Tiles { get; set; } = new();

    public List<Robot> Robots { get; set; } = new();

    public List<Recycler> Recyclers { get; set; } = new();
}

public class Robot
{
    public Player Player { get; set; }

    public Tile Tile { get; set; }
}

public class Recycler
{
    public Player Player { get; set; }

    public Tile Tile { get; set; }
}

public class Player
{
    public bool IsMe { get; set; }

    public int Matter { get; set; }
}

class Game
{
    public static Player Me = new() { IsMe = true };
    public static Player Opponent = new() { IsMe = false };
    public static Map Map = new();

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        foreach (var x in Enumerable.Range(0, width))
        {
            foreach (var y in Enumerable.Range(0, height))
            {
                Map.Tiles.Add(new Tile{ Position = new Position(x, y)});
            }
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            Me.Matter = int.Parse(inputs[0]);
            Opponent.Matter = int.Parse(inputs[1]);
            Map.Recyclers.Clear();
            Map.Robots.Clear();

            foreach (var y in Enumerable.Range(0, height))
            {
                foreach (var x in Enumerable.Range(0, width))
                {
                    inputs = Console.ReadLine().Split(' ');
                    var tile = Map.Tiles.Single(_ => _.Position.X == x && _.Position.Y == y);
                    tile.ScrapAmount = int.Parse(inputs[0]);
                    tile.Player = int.Parse(inputs[1]) == 1 ? Me : int.Parse(inputs[1]) == 0 ? Opponent : null;
                    
                    var units = int.Parse(inputs[2]);
                    foreach (var i in Enumerable.Range(0, units))
                    {
                        Map.Robots.Add(new Robot{Player = Me, Tile = tile});
                    }

                    var recycler = int.Parse(inputs[3]);
                    if (recycler == 1)
                        Map.Recyclers.Add(new Recycler{Player = Me, Tile = tile});

                    tile.CanBuild = int.Parse(inputs[4]) == 1;
                    tile.CanSpawn = int.Parse(inputs[5]) == 1;
                    tile.InRangeOfRecycler = int.Parse(inputs[6]) == 1;
                }
            }

            DecideNextMove();
        }
    }

    private static void DecideNextMove()
    {
        Console.WriteLine("WAIT");
    }
}