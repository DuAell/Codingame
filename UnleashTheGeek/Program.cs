﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace UnleashTheGeek
{

    enum EntityType
    {
        NONE = -1,
        MY_ROBOT = 0,
        OPPONENT_ROBOT = 1,
        RADAR = 2,
        TRAP = 3,
        ORE = 4
    }

    class Cell : Coord
    {
        public int Ore { get; set; }
        public bool Hole { get; set; }
        public bool Known { get; set; }

        public void Update(string ore, int hole)
        {
            Hole = hole == 1;
            Known = !"?".Equals(ore);
            if (Known)
            {
                Ore = int.Parse(ore);
            }
        }

        public Cell(int x, int y) : base(x, y)
        {
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }

    class Game
    {
        // Given at startup
        public readonly int Width;
        public readonly int Height;

        // Updated each turn
        public List<Robot> MyRobots { get; set; }
        public List<Robot> OpponentRobots { get; set; }
        public Cell[,] Cells { get; set; }
        public int RadarCooldown { get; set; }
        public int TrapCooldown { get; set; }
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }
        public List<Entity> Radars { get; set; }
        public List<Entity> Traps { get; set; }

        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            MyRobots = new List<Robot>();
            OpponentRobots = new List<Robot>();
            Cells = new Cell[width, height];
            Radars = new List<Entity>();
            Traps = new List<Entity>();

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    Cells[x, y] = new Cell(x, y);
                }
            }
        }
    }

    class Coord
    {
        public static readonly Coord NONE = new Coord(-1, -1);

        public int X { get; }
        public int Y { get; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Manhattan distance (for 4 directions maps)
        // see: https://en.wikipedia.org/wiki/Taxicab_geometry
        public int Distance(Coord other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this.GetType() != obj.GetType()) return false;
            Coord other = (Coord) obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return 31 * (31 + X) + Y;
        }
    }

    class Entity
    {
        public int Id { get; set; }
        public Coord Pos { get; set; }
        public EntityType Item { get; set; }

        public Entity(int id, Coord pos, EntityType item)
        {
            Id = id;
            Pos = pos;
            Item = item;
        }
    }

    class Robot : Entity
    {
        public Robot(int id, Coord pos, EntityType item) : base(id, pos, item)
        {
        }

        bool IsDead()
        {
            return Pos.Equals(Coord.NONE);
        }

        public string Wait(string message = "")
        {
            return $"WAIT {message}";
        }

        public string Move(Coord pos, string message = "")
        {
            return $"MOVE {pos.X} {pos.Y} {message}";
        }

        public string Dig(Coord pos, string message = "")
        {
            return $"DIG {pos.X} {pos.Y} {message}";
        }

        public string Request(EntityType item, string message = "")
        {
            return $"REQUEST {item} {message}";
        }
    }

/**
 * Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
 **/
    class Player
    {
        static void Main(string[] args)
        {
            new Player();
        }

        Game game;

        public Player()
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]); // size of the map

            game = new Game(width, height);

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                game.MyScore = int.Parse(inputs[0]); // Amount of ore delivered
                game.OpponentScore = int.Parse(inputs[1]);
                for (int i = 0; i < height; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    for (int j = 0; j < width; j++)
                    {
                        string ore = inputs[2 * j]; // amount of ore or "?" if unknown
                        int hole = int.Parse(inputs[2 * j + 1]); // 1 if cell has a hole
                        if (j == 1 && i == 8)
                            Console.Error.WriteLine($"Cell {game.Cells[j, i]}: Known: {game.Cells[j, i].Known}, Ore: {game.Cells[j, i].Ore}");
                        game.Cells[j, i].Update(ore, hole);
                        if (j == 1 && i == 8)
                            Console.Error.WriteLine($"Cell {game.Cells[j, i]}: Known: {game.Cells[j, i].Known}, Ore: {game.Cells[j, i].Ore}");
                    }
                }

                inputs = Console.ReadLine().Split(' ');
                int entityCount = int.Parse(inputs[0]); // number of entities visible to you
                game.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
                game.TrapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
                game.Radars.Clear();
                game.Traps.Clear();
                game.MyRobots.Clear();
                game.OpponentRobots.Clear();
                for (int i = 0; i < entityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int id = int.Parse(inputs[0]); // unique id of the entity
                    EntityType
                        type = (EntityType) int.Parse(
                            inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]); // position of the entity
                    EntityType
                        item = (EntityType) int.Parse(
                            inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
                    Coord coord = new Coord(x, y);
                    switch (type)
                    {
                        case EntityType.MY_ROBOT:
                            game.MyRobots.Add(new Robot(id, coord, item));
                            break;
                        case EntityType.OPPONENT_ROBOT:
                            game.OpponentRobots.Add(new Robot(id, coord, item));
                            break;
                        case EntityType.RADAR:
                            game.Radars.Add(new Entity(id, coord, item));
                            break;
                        case EntityType.TRAP:
                            game.Traps.Add(new Entity(id, coord, item));
                            break;
                    }
                }

                GameLoop();
            }
        }

        public void GameLoop()
        {
            foreach (var myRobot in game.MyRobots)
            {
                var action = ActionSelection(game, myRobot) ?? myRobot.Wait("Default action");

                Console.WriteLine(action);
            }
        }

        public string ActionSelection(Game game, Robot robot)
        {
            // Implement action selection logic here.

            if (robot.Pos.X == 0)
            {
                if (robot.Item == EntityType.ORE)
                {
                    return null;
                }
                //if (robot.Item == EntityType.NONE && game.RadarCooldown == 0)
                //{
                //    game.RadarCooldown = 5;
                //    return robot.Request(EntityType.RADAR);
                //}
            }

            if (robot.Item == EntityType.ORE)
            {
                return robot.Move(new Coord(0, robot.Pos.Y), "Going back to base with crystal");
            }

            var currentCell = game.Cells[robot.Pos.X, robot.Pos.Y];

            if (currentCell.Ore > 0)
            {
                return robot.Dig(robot.Pos);
            }

            var cellsByDistance = game.Cells.OfType<Cell>().OrderBy(x => x.Distance(robot.Pos));

            if (cellsByDistance.Any(x => x.Ore > 0))
            {
                var cell = cellsByDistance.First(x => x.Ore > 0);
                cell.Hole = true;
                return robot.Dig(cell, $"Going to dig crystal out of {cell}");
            }
            else
            {
                var cell = cellsByDistance.First(x => x.X > 0 && !x.Hole);
                Console.Error.WriteLine($"Cell {cell}: Known {cell.Known}, Ore: {cell.Ore}");
                cell.Hole = true;
                cell.Known = true;
                return robot.Dig(cell, $"Going to search in {cell}");
            }

            // WAIT|MOVE x y|REQUEST item|DIG x y

            return null;
        }
    }
}