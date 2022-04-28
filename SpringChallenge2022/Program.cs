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

    public const int MidGame = 100;
    public const int EndGame = 150;

    public static Position MyBase;
    public static Position EnnemyBase;
    public static int MyMana;
    public static int MyHealth;
    public static int OppMana;
    public static int OppHealth;
    public static int Turn = 0;

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
                NumberOfTurnsBeforeHittingTarget = (int)Math.Ceiling((decimal)(Position.Euclidian(MyBase) - DistanceToHitBase) / MonsterMoveRange);
            if (ThreatFor == 2)
                NumberOfTurnsBeforeHittingTarget = (int)Math.Ceiling((decimal)(Position.Euclidian(EnnemyBase) - DistanceToHitBase) / MonsterMoveRange);
        }
    }

    public class Hero : Entity
    {
        public Monster Target;
        public Position LastPosition;
        public bool HasTakenInitialPosition { get; private set; }
        public Position InitialPosition { get; private set; } = new Position(0, 0);

        public int Leash;

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

            if (Position.XY == InitialPosition.XY)
                HasTakenInitialPosition = true;
        }

        public void SetInitialPosition(Position position)
        {
            if (position.XY == InitialPosition.XY)
                return;

            InitialPosition = position;
            HasTakenInitialPosition = false;
        }

        public int GetNumbersOfMonstersInAttackRange(IEnumerable<Monster> monsters)
        {
            return monsters.Count(_ => Position.Euclidian(_.Position) <= HeroAttackRange);
        }

        public Monster GetMonsterClosestToMyBase(IEnumerable<Monster> monsters)
        {
            return monsters.Where(_ => _.ThreatFor == 1).OrderByDescending(_ => _.NearBase).OrderBy(_ => _.Position.Euclidian(Position)).FirstOrDefault();
        }

        public IOrderedEnumerable<T> GetClosest<T>(IEnumerable<T> entities) where T : Entity
        {
            return entities.OrderBy(_ => _.Position.Euclidian(Position));
        }

        public void Attack(Monster monster, string message = null)
        {
            Console.WriteLine($"MOVE {monster.Position} {message} {monster.Id}");
            Target = monster;
        }

        public void Spell(string args, string message = null)
        {
            Console.WriteLine($"SPELL {args} {message}");
            MyMana -= 10;
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
            #region Game data
            Turn++;
            inputs = Console.ReadLine().Split(' ');
            MyHealth = int.Parse(inputs[0]); // Your base health
            MyMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            OppHealth = int.Parse(inputs[0]);
            OppMana = int.Parse(inputs[1]);

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
                Console.Error.WriteLine($"{monster.Id} will hit {(monster.ThreatFor == 1 ? "my base" : "ennemy base")} in {monster.NumberOfTurnsBeforeHittingTarget} turns (Dist: {monster.Position.Euclidian(monster.ThreatFor == 1 ? MyBase : EnnemyBase)})");
            }
            #endregion

            foreach (var hero in myHeroes)
            {
                Console.Error.WriteLine($"Distance to my base: {hero.Position.Euclidian(MyBase)}");

                if (!monsters.Contains(hero.Target))
                    hero.Target = null;

                Monster target = null;

                #region Emergency
                // If a monster is too close to base, push him out
                if (MyMana >= 10 && monsters.Any(_ => _.Position.Euclidian(hero.Position) <= SpellWindRange && _.ShieldLife == 0 && _.NumberOfTurnsBeforeHittingTarget <= 3))
                {
                    hero.Spell($"WIND {EnnemyBase}", "Move out !");
                    continue;
                }

                // Monster in really close to base, emergency attack 
                var emergencyMonster = hero.GetClosest(monsters).Where(_ => _.NumberOfTurnsBeforeHittingTarget <= 7 && _.Position.Euclidian(hero.Position) < HeroMoveRange / _.NumberOfTurnsBeforeHittingTarget).FirstOrDefault();
                if (emergencyMonster != null)
                {
                    hero.Attack(emergencyMonster, "Emergency Attack");
                    continue;
                }

                // Monster is in base attraction range, urgent attack
                var closestApprochingMonster = hero.GetClosest(monsters).Where(_ => _.NearBase == 1 && _.ThreatFor == 1 && _.Position.Euclidian(hero.Position) < HeroMoveRange / _.NumberOfTurnsBeforeHittingTarget).FirstOrDefault();
                if (closestApprochingMonster != null)
                {
                    hero.Attack(closestApprochingMonster, "Urgent Attack");
                    continue;
                }
                #endregion

                #region Attack
                if (myHeroes[2] == hero)
                {
                    Console.Error.WriteLine($"Distance to ennemy heroes: {string.Join(", ", oppHeroes.Select(_ => $"{_.Id}: {_.Position.Euclidian(hero.Position)}"))}");

                    // Farm in middle of the map until mid game
                    if (Turn < MidGame)
                    {
                        hero.SetInitialPosition(new Position(maxX / 2, maxY / 2));

                        //if (hero.Target != null)
                        //{
                        //    Console.WriteLine($"MOVE {hero.Target.Position} (A) Pursue {hero.Target.Id}");
                        //    continue;
                        //}
                        if (hero.HasTakenInitialPosition)
                        {
                            // Attack closest monster
                            target = hero.GetClosest(monsters.Where(_ => _.ThreatFor != 2)).FirstOrDefault();
                            if (target != null)
                            {
                                hero.Attack(target, "(A) Attack");
                                continue;
                            }
                        }                        

                        Console.WriteLine($"MOVE {hero.InitialPosition} (A) Positioning");
                        continue;
                    }
                    else
                    {
                        hero.SetInitialPosition(new Position(Math.Abs(EnnemyBase.X - 3500), Math.Abs(EnnemyBase.Y - 3500)));

                        // Then go to opposite side of ennemy base (same Y)
                        // Go towards ennemy base while CONTROL all monster found on the way
                        // When close to ennemy base, apply shield to all monsters
                        // Overwhelm ennemy heroes and WIND 
                    }

                    var unshieldedMonsterCloseToEnnemyBase = hero.GetClosest(monsters.Where(_ => _.ThreatFor == 2 && _.ShieldLife == 0 && oppHeroes.Any(h => h.Position.Euclidian(_.Position) <= SpellWindRange))).FirstOrDefault();
                    if (unshieldedMonsterCloseToEnnemyBase != null && MyMana >= 10 && OppMana >= 10)
                    {
                        hero.Spell($"SHIELD {unshieldedMonsterCloseToEnnemyBase.Id}", "Shield spider");
                        continue;
                    }

                    // If there is a pack of monsters, push them to ennemy base
                    if (MyMana >= 10 && hero.GetClosest(monsters.Where(_ => _.ThreatFor == 2 && _.ShieldLife == 0)).Count() >= 3)
                    {
                        hero.Spell($"WIND {EnnemyBase}", "(A) Push pack to ennemy base");
                        continue;
                    }

                    // Push monsters towards ennemy base without pushing ennemy heroes
                    if (MyMana >= 10 
                        && monsters.Any(_ => _.Position.Euclidian(hero.Position) <= SpellWindRange && _.ShieldLife == 0 && _.Position.Euclidian(EnnemyBase) <= BaseAttractionRange + SpellWindPushForce)
                        && !oppHeroes.Any(_ => _.Position.Euclidian(hero.Position) <= SpellWindRange))
                    {
                        hero.Spell($"WIND {EnnemyBase}", "(A) Push to ennemy base");
                        continue;
                    }

                    // Disengage ennemy heroes of monsters
                    var ennemyHero = oppHeroes.Where(_ => _.Position.Euclidian(hero.Position) <= SpellControlRange && _.ShieldLife == 0 && _.GetNumbersOfMonstersInAttackRange(monsters) > 0 && _.Position.Euclidian(EnnemyBase) < 7000).OrderByDescending(_ => _.GetNumbersOfMonstersInAttackRange(monsters)).FirstOrDefault();
                    if (MyMana >= 10 && ennemyHero != null)
                    {
                        hero.Spell($"CONTROL {ennemyHero.Id} {MyBase}", $"(A) Control {ennemyHero.Id}");
                        continue;
                    }

                    if (Turn > 150)
                    {
                        // Focus on removing ennemy heroes from their defense positions
                        ennemyHero = oppHeroes.Where(_ => _.Position.Euclidian(hero.Position) <= SpellControlRange && _.ShieldLife == 0 && _.Position.Euclidian(EnnemyBase) < 7000).OrderByDescending(_ => _.GetNumbersOfMonstersInAttackRange(monsters)).FirstOrDefault(); // Compute numbers of monsters in range
                        if (MyMana >= 10 && ennemyHero != null)
                        {
                            hero.Spell($"CONTROL {ennemyHero.Id} {MyBase}", $"(A) Control {ennemyHero.Id}");
                            continue;
                        }
                    }

                    // Getting close to monster without attacking him
                    var closestMonsterOfEnnemyBase = monsters.Where(_ => _.NearBase == 1 && _.ShieldLife == 0 && _.ThreatFor == 2 && _.Position.Euclidian(hero.Position) > HeroMoveRange).OrderBy(m => m.Position.Euclidian(hero.Position)).FirstOrDefault();
                    if (MyMana >= 10 && closestMonsterOfEnnemyBase != null)
                    {
                        Console.WriteLine($"MOVE {closestMonsterOfEnnemyBase.Position} (A) Getting close of {closestMonsterOfEnnemyBase.Id}");
                        continue;
                    }

                    //var monstersNotGoingToEnnemyBase = monsters.Where(_ => _.Position.Euclidian(hero.Position) < SpellControlRange && _.ShieldLife == 0 && _.ThreatFor != 2 && _.Health > 10);
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

                    Console.WriteLine($"MOVE {hero.InitialPosition} (A) Positioning");
                    continue;
                }
                #endregion

                #region Defense
                if (myHeroes[1] == hero)
                {
                    hero.SetInitialPosition(new Position(Math.Abs(MyBase.X - 4600), Math.Abs(MyBase.Y - 1700)));
                    hero.Leash = 8000;
                }
                else
                {
                    hero.SetInitialPosition(new Position(Math.Abs(MyBase.X - 2400), Math.Abs(MyBase.Y - 4400)));
                    if (Turn <= 50)
                        hero.Leash = 15000;
                    else
                        hero.Leash = 10000;
                }                   

                if (hero.Position.Euclidian(MyBase) < hero.Leash) //myHeroes[0] == hero || myHeroes[1] == hero && hero.Position.Euclidian(myBase) < 9000 || myHeroes[2] == hero && hero.Position.Euclidian(myBase) < 7000) // Leash
                {
                    if (hero.Target != null && hero.Target.NearBase == 1 && hero.Target.ThreatFor == 1)
                    {
                        hero.Attack(hero.Target, "(D) Pursue");
                        continue;
                    }

                    // Always protect my heroes
                    if (MyMana >= 10 && hero.ShieldLife == 0 && oppHeroes.Any(_ => _.Position.Euclidian(hero.Position) <= SpellControlRange + HeroMoveRange) && OppMana >= 10)
                    {
                        hero.Spell($"SHIELD {hero.Id}");
                        continue;
                    }

                    var monstersCloseToMyBase = monsters.Where(_ => _.Position.Euclidian(hero.Position) <= SpellControlRange && _.ShieldLife == 0 && !(_.ThreatFor == 1 && _.NearBase == 1) && _.Health > 20).OrderBy(_ => _.NumberOfTurnsBeforeHittingTarget).ThenByDescending(_ => _.ThreatFor);
                    if (MyMana >= 10 && monstersCloseToMyBase.Any())
                    {
                        var monster = monstersCloseToMyBase.First();
                        hero.Target = monster;
                        monster.ThreatFor = 2;
                        monster.NearBase = 0;
                        hero.Spell($"CONTROL {monster.Id} {EnnemyBase}", $"(D) Control {monster.Id}");
                        continue;
                    }

                    var untargetedMonsters = monsters.Where(_ => !myHeroes.Select(h => h.Target).Any(p => p == _));

                    target = hero.GetMonsterClosestToMyBase(untargetedMonsters) ?? hero.GetMonsterClosestToMyBase(monsters);

                    if (target != null)
                    {
                        hero.Attack(target, "(D) Attack target");
                        continue;
                    }
                }

                Console.WriteLine($"MOVE {hero.InitialPosition} (D) Positioning");
                #endregion

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

        public int Euclidian(Position p2) => (int)Math.Sqrt(Math.Pow(X - p2.X, 2) + Math.Pow(Y - p2.Y, 2));

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