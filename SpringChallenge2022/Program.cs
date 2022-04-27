using System;
using System.Collections.Generic;
using System.Linq;

class Player
{
    public const int TYPE_MONSTER = 0;
    public const int TYPE_MY_HERO = 1;
    public const int TYPE_OP_HERO = 2;

    public const int SpellWindRange = 1280;
    public const int SpellWindPushForce = 2200;
    public const int SpellShieldRange = 2200;
    public const int SpellControlRange = 2200;
    public const int HeroMoveRange = 800;
    public const int HeroAttackRange = 800;
    public const int MonsterMoveRange = 400;
    public const int BaseAttractionRange = 5000;
    public const int DistanceToHitBase = 300;

    public static Position MyBase;
    public static Position EnnemyBase;

    public abstract class Entity
    {
        public int Id;
        public int Type;
        public int X, Y;
        public int ShieldLife;
        public int IsControlled;
        public Position Position;
        
        public Entity(int id, int type, int x, int y, int shieldLife, int isControlled)
        {
            
        }

        protected void InternalUpdate(int id, int type, int x, int y, int shieldLife, int isControlled)
        {
            Id = id;
            Type = type;
            X = x;
            Y = y;
            ShieldLife = shieldLife;
            IsControlled = isControlled;
            Position = new Position(x, y);
        }
    }

    public class Monster : Entity
    {
        public int Health;
        public int Vx, Vy;
        public int NearBase;
        public int ThreatFor;
        public Position Target;
        public int NumberOfTurnsBeforeHittingTarget;

        public Monster(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor) : base(id, type, x, y, shieldLife, isControlled)
        {
            Update(id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor);
        }

        public void Update(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor)
        {
            InternalUpdate(id, type, x, y, shieldLife, isControlled);

            Health = health;
            Vx = vx;
            Vy = vy;
            NearBase = nearBase;
            ThreatFor = threatFor;
            Target = new Position(vx, vy);

            if (ThreatFor == 0)
                NumberOfTurnsBeforeHittingTarget = 999;
            if (ThreatFor == 1)
                NumberOfTurnsBeforeHittingTarget = (int)Math.Ceiling((decimal)(Position.Manhattan(MyBase) - DistanceToHitBase) / MonsterMoveRange);
            if (ThreatFor == 2)
                NumberOfTurnsBeforeHittingTarget = (int)Math.Ceiling((decimal)(Position.Manhattan(EnnemyBase) - DistanceToHitBase) / MonsterMoveRange);
        }
    }

    public class Hero : Entity
    {
        public Monster Target;
        public Position LastPosition;
        public bool HasTakenInitialPosition;

        public Hero(int id, int type, int x, int y, int shieldLife, int isControlled) : base(id, type, x, y, shieldLife, isControlled)
        {
            Update(id, type, x, y, shieldLife, isControlled);
        }

        public void Update(int id, int type, int x, int y, int shieldLife, int isControlled)
        {
            if (LastPosition == null || X != x || Y != y)
            {
                LastPosition = new Position(X, Y);
            }

            InternalUpdate(id, type, x, y, shieldLife, isControlled);
        }

        public int GetNumbersOfMonstersInAttackRange(IEnumerable<Monster> monsters)
        {
            return monsters.Count(_ => Position.Manhattan(_.Position) <= HeroAttackRange);
        }

        public Monster GetClosestMonster(IEnumerable<Monster> monsters)
        {
            return monsters.Where(_ => _.ThreatFor == 1).OrderByDescending(_ => _.NearBase).OrderBy(m => m.Position.Manhattan(Position)).FirstOrDefault();
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        const int maxX = 17630;
        const int maxY = 9000;

        // base_x,base_y: The corner of the map representing your base
        int baseX = int.Parse(inputs[0]);
        int baseY = int.Parse(inputs[1]);

        MyBase = new Position(baseX, baseY);
        EnnemyBase = new Position(baseX == 0 ? maxX : 0, baseX == 0 ? maxY : 0);

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        var myHeroes = new List<Hero>();
        var oppHeroes = new List<Hero>();
        var monsters = new List<Monster>();

        // game loop
        while (true)
        {

            inputs = Console.ReadLine().Split(' ');
            int myHealth = int.Parse(inputs[0]); // Your base health
            int myMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            int oppHealth = int.Parse(inputs[0]);
            int oppMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see
            var entityIds = new List<int>();

            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                entityIds.Add(id);
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                switch (type)
                {
                    case TYPE_MONSTER:
                        var monster = monsters.FirstOrDefault(_ => _.Id == id);
                        if (monster == null)
                            monsters.Add(new Monster(id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor));
                        else
                            monster.Update(id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor);
                        break;
                    case TYPE_MY_HERO:
                        var hero = myHeroes.FirstOrDefault(_ => _.Id == id);
                        if (hero == null)
                            myHeroes.Add(new Hero(id, type, x, y, shieldLife, isControlled));
                        else
                            hero.Update(id, type, x, y, shieldLife, isControlled);
                        break;
                    case TYPE_OP_HERO:
                        hero = oppHeroes.FirstOrDefault(_ => _.Id == id);
                        if (hero == null)
                            oppHeroes.Add(new Hero(id, type, x, y, shieldLife, isControlled));
                        else
                            hero.Update(id, type, x, y, shieldLife, isControlled);
                        break;
                }
            }

            // Remove killed entities
            monsters.RemoveAll(_ => !entityIds.Contains(_.Id));
            myHeroes.RemoveAll(_ => !entityIds.Contains(_.Id));
            oppHeroes.RemoveAll(_ => !entityIds.Contains(_.Id));

            foreach (var monster in monsters.Where(_ => _.NumberOfTurnsBeforeHittingTarget < 15))
            {
                Console.Error.WriteLine($"{monster.Id} will hit {(monster.ThreatFor == 1 ? "my base" : "ennemy base")} in {monster.NumberOfTurnsBeforeHittingTarget} turns (Dist: {monster.Position.Manhattan(monster.ThreatFor == 1 ? MyBase : EnnemyBase)})");
            }

            foreach (var hero in myHeroes)
            {
                if (!monsters.Contains(hero.Target))
                    hero.Target = null;

                Monster target = null;

                if (myHeroes[0] == hero)
                {
                    // Attack

                    // Push monsters towards ennemy base
                    if (myMana >= 10 && monsters.Any(_ => _.Position.Manhattan(hero.Position) < SpellWindRange && _.ShieldLife == 0 && _.Position.Manhattan(EnnemyBase) < BaseAttractionRange + SpellWindPushForce))
                    {
                        Console.WriteLine($"SPELL WIND {EnnemyBase} (A) Push to ennemy base");
                        myMana -= 10;
                        continue;
                    }

                    // Disengage ennemy heroes of monsters
                    var ennemyHero = oppHeroes.Where(_ => _.Position.Manhattan(hero.Position) < SpellControlRange && _.GetNumbersOfMonstersInAttackRange(monsters) > 0).OrderByDescending(_ => _.GetNumbersOfMonstersInAttackRange(monsters)).FirstOrDefault(); // Compute numbers of monsters in range
                    if (myMana >= 10 && ennemyHero != null)
                    {
                        Console.WriteLine($"SPELL CONTROL {ennemyHero.Id} {MyBase} (A) Control {ennemyHero.Id}");
                        myMana -= 10;
                        continue;
                    }

                    // Getting close to monster without attacking him
                    var closestMonsterOfEnnemyBase = monsters.Where(_ => _.NearBase == 1 && _.ShieldLife == 0 && _.ThreatFor == 2 && _.Position.Manhattan(hero.Position) > HeroMoveRange).OrderBy(m => m.Position.Manhattan(hero.Position)).FirstOrDefault();
                    if (myMana >= 10 && closestMonsterOfEnnemyBase != null)
                    {
                        Console.WriteLine($"MOVE {closestMonsterOfEnnemyBase.Position} (A) Getting close of {closestMonsterOfEnnemyBase.Id}");
                        myMana -= 10;
                        continue;
                    }

                    //var monstersNotGoingToEnnemyBase = monsters.Where(_ => _.Position.Manhattan(hero.Position) < SpellControlRange && _.ShieldLife == 0 && _.ThreatFor != 2 && _.Health > 10);
                    //if (myMana >= 10 && monstersNotGoingToEnnemyBase.Any())
                    //{
                    //    var monster = monstersNotGoingToEnnemyBase.First();
                    //    monster.ThreatFor = 2;
                    //    monster.NearBase = 0;
                    //    Console.WriteLine($"SPELL CONTROL {monster.Id} {ennemyBase} (A) This way !");
                    //    myMana -= 10;
                    //    continue;
                    //}

                    //// Going up and down in the middle of the map
                    //if (hero.Position.X == 8900)
                    //{
                    //    Console.Error.WriteLine("Last position: " + hero.LastPosition);
                    //    var direction = hero.Position.Y > hero.LastPosition.Y ? Direction.South : Direction.North;
                    //    if (maxY - hero.Position.Y < HeroMoveRange || hero.Position.Y < HeroMoveRange)
                    //    {
                    //        direction = direction == Direction.South ? Direction.North : Direction.South;
                    //        Console.Error.WriteLine("Change direction");
                    //    }
                    //    Console.WriteLine($"MOVE 8900 {(direction == Direction.North ? 0 : maxY)} (A) Moving");
                    //    continue;
                    //}

                    var attackDefaultPosition = new Position(Math.Abs(EnnemyBase.X - 3500), Math.Abs(EnnemyBase.Y - 3500));

                    if (hero.Position.XY == attackDefaultPosition.XY)
                        hero.HasTakenInitialPosition = true;

                    // Roaming around ennemy base
                    if (hero.HasTakenInitialPosition)
                    {
                        var direction = hero.Position.Y > hero.LastPosition.Y ? Direction.South : Direction.North;
                        Console.Error.WriteLine($"Position: {hero.Position}, Last: {hero.LastPosition}, Direction: {Enum.GetName(typeof(Direction), direction)}");
                        if (maxX - hero.Position.X < HeroMoveRange || hero.Position.Y < HeroMoveRange)
                            direction = Direction.South;
                        else if (maxY - hero.Position.Y < HeroMoveRange || hero.Position.X < HeroMoveRange)
                            direction = Direction.North;

                        if (EnnemyBase.X == 0)
                            Console.WriteLine($"MOVE {(direction == Direction.North ? 6000 : 0)} {(direction == Direction.North ? 0 : 6000)} (A) Roaming");
                        else
                            Console.WriteLine($"MOVE {(direction == Direction.North ? EnnemyBase.X : Math.Abs(EnnemyBase.X - 6000))} {(direction == Direction.North ? Math.Abs(EnnemyBase.Y - 6000) : EnnemyBase.Y)} (A) Roaming");
                        continue;
                    }

                    Console.WriteLine($"MOVE {attackDefaultPosition} (A) Positioning");
                }
                else
                {
                    // Defense
                    if (hero.Position.Manhattan(MyBase) < 9000) //myHeroes[0] == hero || myHeroes[1] == hero && hero.Position.Manhattan(myBase) < 9000 || myHeroes[2] == hero && hero.Position.Manhattan(myBase) < 7000) // Leash
                    {
                        if (myMana >= 10 && monsters.Any(_ => _.Position.Manhattan(hero.Position) < SpellWindRange && _.NumberOfTurnsBeforeHittingTarget <= 3))
                        {
                            Console.WriteLine($"SPELL WIND {EnnemyBase} (D) Move out !");
                            myMana -= 10;
                            continue;
                        }

                        if (hero.Target != null && hero.Target.NearBase == 1 && hero.Target.ThreatFor == 1)
                        {
                            Console.WriteLine($"MOVE {hero.Target.Position} (D) Pursue target {hero.Target.Id}");
                            continue;
                        }

                        var monstersCloseToMyBase = monsters.Where(_ => _.Position.Manhattan(hero.Position) < SpellControlRange && _.Position.Manhattan(MyBase) > BaseAttractionRange && _.ShieldLife == 0 && _.ThreatFor != 2 && _.Health > 20).OrderBy(_ => _.NumberOfTurnsBeforeHittingTarget).ThenByDescending(_ => _.ThreatFor);
                        if (myMana >= 10 && monstersCloseToMyBase.Any())
                        {
                            var monster = monstersCloseToMyBase.First();
                            hero.Target = monster;
                            monster.ThreatFor = 2;
                            monster.NearBase = 0;
                            Console.WriteLine($"SPELL CONTROL {monster.Id} {EnnemyBase} (D) Control {monster.Id}");
                            myMana -= 10;
                            continue;
                        }

                        var untargetedMonsters = monsters.Where(_ => !myHeroes.Select(h => h.Target).Any(p => p == _));

                        target = hero.GetClosestMonster(untargetedMonsters) ?? hero.GetClosestMonster(monsters);

                        if (target != null)
                        {
                            hero.Target = target;
                            Console.WriteLine($"MOVE {hero.Target.Position} (D) Attack target {hero.Target.Id}");
                            continue;
                        }
                    }

                    if (myHeroes[1] == hero)
                        Console.WriteLine($"MOVE {Math.Abs(MyBase.X - 4600)} {Math.Abs(MyBase.Y - 1700)} (D) Positioning");
                    else
                        Console.WriteLine($"MOVE {Math.Abs(MyBase.X - 2400)} {Math.Abs(MyBase.Y - 4400)} (D) Positioning");
                }

                //if (myHeroes[0] == hero)
                //    Console.WriteLine($"MOVE {Math.Abs(MyBase.X - 5800)} {Math.Abs(MyBase.Y - 500)} Positioning");
                //else if (myHeroes[1] == hero)
                //    Console.WriteLine($"MOVE {Math.Abs(MyBase.X - 4800)} {Math.Abs(MyBase.Y - 3400)} Positioning");
                //else
                //    Console.WriteLine($"MOVE {Math.Abs(MyBase.X - 1900)} {Math.Abs(MyBase.Y - 5300)} Positioning");
            }
        }
    }

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

        public override string ToString()
        {
            return XY;
        }
    }

    public enum Direction
    {
        North,
        South,
        East,
        West
    }
}