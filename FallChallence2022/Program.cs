using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

    public bool WontBeGrass => !WillBeGrass;

    public bool WillBeGrass => InRangeOfRecycler && ScrapAmount <= 1;

    public IEnumerable<Tile> GetNeighbors(Map map)
    {
        return map.Tiles.Where(_ =>
            _ != this && 
            (Math.Abs(Position.X - _.Position.X) <= 1 && Position.Y == _.Position.Y) ||
            (Math.Abs(Position.Y - _.Position.Y) <= 1 && Position.X == _.Position.X));
    }
}

public class Map
{
    public List<Tile> Tiles { get; set; } = new();

    public IEnumerable<Tile> TilesThatWontBeGrass => Tiles.Where(_ => _.WontBeGrass);

    public IEnumerable<Tile> TilesThatWillBeGrass => Tiles.Where(_ => _.WillBeGrass);

    public List<Robot> Robots { get; set; } = new();

    public List<Recycler> Recyclers { get; set; } = new();
}

public class Robot
{
    public Player Player { get; set; }

    public Tile Tile { get; set; }
    public Tile NextTile { get; set; }
    public bool HasMoved => NextTile != null;

    public List<Robot> OtherRobotsOnSameTile => Game.Map.Robots.Where(_ => _ != this && _.Tile == Tile).ToList();

    public void Move(int count, Tile tile)
    {
        Game.Actions.Add($"MOVE {count} {Tile.Position.XY} {tile.Position.XY}");
        NextTile = tile;
        OtherRobotsOnSameTile.Take(count - 1).ToList().ForEach(_ => _.NextTile = tile);
    }
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
    public static List<string> Actions = new();
    public static int Turn;

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
            Actions.Clear();
            Turn++;

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
                        Map.Robots.Add(new Robot{Player = tile.Player, Tile = tile});
                    }

                    var recycler = int.Parse(inputs[3]);
                    if (recycler == 1)
                        Map.Recyclers.Add(new Recycler{Player = tile.Player, Tile = tile});

                    tile.CanBuild = int.Parse(inputs[4]) == 1;
                    tile.CanSpawn = int.Parse(inputs[5]) == 1;
                    tile.InRangeOfRecycler = int.Parse(inputs[6]) == 1;
                }
            }

            DecideNextMove();

            Console.Error.WriteLine($"Will be grass: {string.Join(",", Map.TilesThatWillBeGrass.Select(_ => _.Position.XY))}");
        }
    }

    private static void DecideNextMove()
    {
        var myRobots = Map.Robots.Where(_ => _.Player == Me).ToList();
        var opponentRobots = Map.Robots.Where(_ => _.Player == Opponent).ToList();

        // Move robots out of tiles that will be recycled
        foreach (var robot in myRobots.Where(_ => _.Tile.WillBeGrass))
        {
            var nearestTile = Map.Tiles.Where(_ => _.WontBeGrass).OrderBy(_ => _.Position.Manhattan(robot.Tile.Position)).First();
            robot.Move(1, nearestTile);
        }

        // Attack
        foreach (var robot in myRobots)
        {
            if (robot.HasMoved) continue;

            var nearestOpponent = opponentRobots.Where(_ => _.Tile.WontBeGrass && _.OtherRobotsOnSameTile.Count < robot.OtherRobotsOnSameTile.Count).MinBy(_ => _.Tile.Position.Manhattan(robot.Tile.Position));
            if (nearestOpponent != null)
            {
                robot.Move(nearestOpponent.OtherRobotsOnSameTile.Count + 1, nearestOpponent.Tile);
            }
        }

        var lonelyRobots = myRobots.Where(_ => _.OtherRobotsOnSameTile.Count == 0).Skip(3).ToList();
        // Regroup
        foreach (var lonelyRobot in lonelyRobots)
        {
            if (lonelyRobot.HasMoved) continue;

            var closeFriend = myRobots.FirstOrDefault(_ =>
            {
                var tile = (_.HasMoved ? _.NextTile : _.Tile);
                return tile.WontBeGrass && tile.Position.Manhattan(lonelyRobot.Tile.Position) == 1;
            });
            if (closeFriend != null)
            {
                lonelyRobot.Move(1, closeFriend.Tile);
                closeFriend.NextTile = closeFriend.Tile;
            }
        }

        // Recon
        foreach (var robot in myRobots.Where(_ => !_.HasMoved))
        {
            var nearestTile = Map.Tiles.Where(_ => _.Player != Me && _.WontBeGrass).OrderBy(_ => _.Position.Manhattan(robot.Tile.Position)).First();
            robot.Move(1, nearestTile);
        }

        var originalActionCount = 0;
        while (Me.Matter >= 10 && originalActionCount != Actions.Count)
        {
            originalActionCount = Actions.Count;

            if (Map.Tiles.Count(_ => _.Player == Me) < Map.Tiles.Count(_ => _.Player == Opponent) && Turn > 20) // Moins de tiles que l'adversaire, on arrête de faire des recycleurs pour éviter d'en perdre plus
            {
                if (Map.Recyclers.Count(_ => _.Player == Me) < 2)
                {
                    var tiles = Map.Tiles
                        .Where(_ => _.CanBuild &&
                                    !_.GetNeighbors(Map).Intersect(Map.Recyclers.Select(r => r.Tile)).Any()) // Pas de recycleurs les uns à côté des autres
                        .OrderByDescending(_ => _.GetNeighbors(Map).Sum(t => t.ScrapAmount));
                    var tile = tiles.FirstOrDefault();
                    if (tile != null)
                    {
                        Actions.Add($"BUILD {tile.Position.XY}");
                        Me.Matter -= 10;
                        Map.Recyclers.Add(new Recycler { Player = Me, Tile = tile });
                    }
                }
            }

            if (Map.Robots.Count(_ => _.Player == Me) < 3 || Me.Matter >= 20)
            {
                var robots = Map.Robots
                    .Where(_ => _.Player == Me && _.Tile.CanSpawn &&
                                _.Tile.WontBeGrass &&
                                _.Tile.GetNeighbors(Map).Any(_ => _.ScrapAmount > 1)) // Sinon on spawn sur une case de laquelle on ne pourra pas partir
                    .OrderBy(_ => Map.Robots.Count(r => r.Tile == _.Tile));
                var tile = robots.FirstOrDefault()?.Tile;
                if (tile != null)
                {
                    Actions.Add($"SPAWN 1 {tile.Position.XY}");
                    Me.Matter -= 10;
                    Map.Robots.Add(new Robot { Player = Me, Tile = tile });
                }
            }
        }

        Console.WriteLine(Actions.Any() ? string.Join(";", Actions) : "WAIT");
    }
}