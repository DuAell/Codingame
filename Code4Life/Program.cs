using System;
using System.Linq;
using System.Collections.Generic;

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
class Player
{
    private static bool _hasWritten;
    public static Bot Me;
    public static Bot Opponent;
    public static List<Sample> Samples;
    public static List<Molecule> Molecules = new List<Molecule>
    {
        new Molecule("A"),
        new Molecule("B"),
        new Molecule("C"),
        new Molecule("D"),
        new Molecule("E"),
    };
    public static int AvailableA;
    public static int AvailableB;
    public static int AvailableC;
    public static int AvailableD;
    public static int AvailableE;

    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int a = int.Parse(inputs[0]);
            int b = int.Parse(inputs[1]);
            int c = int.Parse(inputs[2]);
            int d = int.Parse(inputs[3]);
            int e = int.Parse(inputs[4]);
        }

        Me = new Bot(true);
        Opponent = new Bot(false);
        Samples = new List<Sample>();

        // game loop
        while (true)
        {
            _hasWritten = false;
            Samples.Clear();

            for (var i = 0; i < 2; i++)
            {
                if (i == 0)
                    Me.Initialize();
                else
                    Opponent.Initialize();
            }
            inputs = Console.ReadLine().Split(' ');
            Molecules.Single(x => x.Type == "A").Available = int.Parse(inputs[0]);
            Molecules.Single(x => x.Type == "B").Available = int.Parse(inputs[1]);
            Molecules.Single(x => x.Type == "C").Available = int.Parse(inputs[2]);
            Molecules.Single(x => x.Type == "D").Available = int.Parse(inputs[3]);
            Molecules.Single(x => x.Type == "E").Available = int.Parse(inputs[4]);
            AvailableA = int.Parse(inputs[0]);
            AvailableB = int.Parse(inputs[1]);
            AvailableC = int.Parse(inputs[2]);
            AvailableD = int.Parse(inputs[3]);
            AvailableE = int.Parse(inputs[4]);
            var sampleCount = int.Parse(Console.ReadLine());
            for (var i = 0; i < sampleCount; i++)
            {
                Samples.Add(new Sample());
            }

            Me.CarriedSamples = Samples.Where(x => x.CarriedBy == 0).ToList();

            Me.Process();
        }
    }

    private static void Write(string text)
    {
        if (_hasWritten) return;
        Console.WriteLine(text);
        _hasWritten = true;
    }

    public enum BotState
    {
        GoingToSamples,
        GoingToDiagnosis,
        GoingToMolecules,
        GoingToLaboratory,
        ArrivedAtSamples,
        ArrivedAtDiagnosis,
        ArrivedAtMolecules,
        ArrivedAtLaboratory,
        SamplesTaken,
        SamplesDiagnosed,
        MoleculesTaken,
        ProcessDone
    }

    public class Bot
    {
        private BotState? _state;

        public Bot(bool isMe)
        {
            IsMe = isMe;
        }

        public void Initialize()
        {
            var inputs = Console.ReadLine().Split(' ');
            Target = inputs[0];
            Eta = int.Parse(inputs[1]);
            Score = int.Parse(inputs[2]);
            StorageA = int.Parse(inputs[3]);
            StorageB = int.Parse(inputs[4]);
            StorageC = int.Parse(inputs[5]);
            StorageD = int.Parse(inputs[6]);
            StorageE = int.Parse(inputs[7]);
            ExpertiseA = int.Parse(inputs[8]);
            ExpertiseB = int.Parse(inputs[9]);
            ExpertiseC = int.Parse(inputs[10]);
            ExpertiseD = int.Parse(inputs[11]);
            ExpertiseE = int.Parse(inputs[12]);

            if (State == BotState.GoingToSamples && Target == "SAMPLES")
                State = BotState.ArrivedAtSamples;
            else if (State == BotState.GoingToDiagnosis && Target == "DIAGNOSIS")
                State = BotState.ArrivedAtDiagnosis;
            else if (State == BotState.GoingToMolecules && Target == "MOLECULES")
                State = BotState.ArrivedAtMolecules;
            else if (State == BotState.GoingToLaboratory && Target == "LABORATORY")
                State = BotState.ArrivedAtLaboratory;
            else
                Console.Error.WriteLine(State);

            foreach (var sample in CarriedSamples)
            {
                var debugMessage = $"Sample {sample.SampleId} : ";
                debugMessage += $"A:{Me.StorageA}/{sample.MyCostA}({sample.CostA}) ";
                debugMessage += $"B:{Me.StorageB}/{sample.MyCostB}({sample.CostB}) ";
                debugMessage += $"C:{Me.StorageC}/{sample.MyCostC}({sample.CostC}) ";
                debugMessage += $"D:{Me.StorageD}/{sample.MyCostD}({sample.CostD}) ";
                debugMessage += $"E:{Me.StorageE}/{sample.MyCostE}({sample.CostE}) ";

                Console.Error.WriteLine(debugMessage);
            }
        }

        public bool CanStoreMolecules()
        {
            return StorageA + StorageB + StorageC + StorageD + StorageE < 10;
        }

        public bool NeedMolecules()
        {
            return NeedA() > 0 || NeedB() > 0 || NeedC() > 0 || NeedD() > 0 || NeedE() > 0;
        }

        public int NeedA()
        {
            return CarriedSamples.Sum(x => x.RealCostA(this)) - StorageA;
        }

        public int NeedB()
        {
            return CarriedSamples.Sum(x => x.RealCostB(this)) - StorageB;
        }

        public int NeedC()
        {
            return CarriedSamples.Sum(x => x.RealCostC(this)) - StorageC;
        }

        public int NeedD()
        {
            return CarriedSamples.Sum(x => x.RealCostD(this)) - StorageD;
        }

        public int NeedE()
        {
            return CarriedSamples.Sum(x => x.RealCostE(this)) - StorageE;
        }

        public bool IsMe { get; }
        public string Target { get; private set; }
        public int Eta { get; private set; }
        public int Score { get; private set; }
        public int StorageA { get; private set; }
        public int StorageB { get; private set; }
        public int StorageC { get; private set; }
        public int StorageD { get; private set; }
        public int StorageE { get; private set; }
        public int ExpertiseA { get; private set; }
        public int ExpertiseB { get; private set; }
        public int ExpertiseC { get; private set; }
        public int ExpertiseD { get; private set; }
        public int ExpertiseE { get; private set; }

        public int TotalExpertise => ExpertiseA + ExpertiseB + ExpertiseC + ExpertiseD + ExpertiseE;
        public int TotalStorage => StorageA + StorageB + StorageC + StorageD + StorageE;

        public BotState? State
        {
            get => _state;
            set
            {
                _state = value;
                Console.Error.WriteLine(value);
            }
        }

        public List<Sample> CarriedSamples { get; set; } = new List<Sample>();

        public void Process()
        {
            if (State == null)
                GotoSamples();
            else
                while (!_hasWritten)
                    StateMachine[State.Value].Invoke();
        }
    }

    public static readonly Dictionary<BotState, Action> StateMachine = new Dictionary<BotState, Action>
    {
        { BotState.GoingToSamples, GotoSamples },
        { BotState.GoingToDiagnosis, GotoDiagnosis },
        { BotState.GoingToMolecules, GotoMolecules },
        { BotState.GoingToLaboratory, GotoLaboratory },
        { BotState.ArrivedAtSamples, OnArrivedAtSamples },
        { BotState.ArrivedAtDiagnosis, OnArrivedAtDiagnosis },
        { BotState.ArrivedAtMolecules, OnArrivedAtMolecules },
        { BotState.ArrivedAtLaboratory, OnArrivedAtLaboratory },
        { BotState.SamplesTaken, OnSamplesTaken},
        { BotState.SamplesDiagnosed, OnSamplesDiagnosed},
        { BotState.MoleculesTaken, OnMoleculesTaken},
        { BotState.ProcessDone, OnProcessDone},
    };

    private static void GotoSamples()
    {
        Write("GOTO SAMPLES");
        Me.State = BotState.GoingToSamples;
    }

    private static void GotoDiagnosis()
    {
        Write("GOTO DIAGNOSIS");
        Me.State = BotState.GoingToDiagnosis;
    }

    private static void GotoMolecules()
    {
        Write("GOTO MOLECULES");
        Me.State = BotState.GoingToMolecules;
    }

    private static void GotoLaboratory()
    {
        Write("GOTO LABORATORY");
        Me.State = BotState.GoingToLaboratory;
    }

    private static void OnArrivedAtSamples()
    {
        Console.Error.WriteLine($"{Samples.Count(x => x.CarriedBy == -1)} samples available. Already carrying {Me.CarriedSamples.Count}");
        if (Me.CarriedSamples.Count < 3)
        {
            if (Me.TotalExpertise > 17)
                Write("CONNECT 3"); // 3 rank 3
            else if (Me.TotalExpertise > 14 && Me.CarriedSamples.Count(x => x.Rank == 3) < 2)
                Write("CONNECT 3"); // 2 rank 3
            else if (Me.TotalExpertise > 11 && Me.CarriedSamples.Count(x => x.Rank == 3) < 1)
                Write("CONNECT 3"); // 1 rank 3
            else if (Me.TotalExpertise > 8)
                Write("CONNECT 2"); // 3 rank 2
            else if (Me.TotalExpertise > 5 && Me.CarriedSamples.Count(x => x.Rank == 2) < 2)
                Write("CONNECT 2"); // 2 rank 2
            else if (Me.TotalExpertise > 2 && Me.CarriedSamples.Count(x => x.Rank == 2) < 1)
                Write("CONNECT 2"); // 1 rank 2
            else
                Write("CONNECT 1");
            return;
        }

        Me.State = BotState.SamplesTaken;
    }

    private static void OnArrivedAtDiagnosis()
    {
        var nonDiagnosedSample = Me.CarriedSamples.FirstOrDefault(x => x.State == SampleState.NonDiagnosed);
        if (nonDiagnosedSample != null)
        {
            nonDiagnosedSample.Diagnose();
            return;
        }

        var cloudSamples = Samples.Where(x => x.CarriedBy == -1 && x.State == SampleState.Diagnosed).ToList();
        if (Me.CarriedSamples.Count < 3)
        {
            if (cloudSamples.Any(x => x.CanBeProcessed()))
            {
                cloudSamples.First(x => x.CanBeProcessed()).Diagnose();
                return;
            }

            var lessCostlySample = cloudSamples.Where(x => !x.IsTooCostly()).OrderBy(x => x.CostLeft()).FirstOrDefault();
            if (lessCostlySample != null)
            {
                lessCostlySample.Diagnose();
                return;
            }
        }

        var sampleTooCostly = Me.CarriedSamples.FirstOrDefault(x => x.IsTooCostly());
        if (sampleTooCostly != null)
        {
            sampleTooCostly.Diagnose();
            return;
        }

        if (!Me.CanStoreMolecules() && Me.CarriedSamples.All(x => !x.CanBeProcessed()))
        {
            Me.CarriedSamples.First(x => x.CostLeft() == Me.CarriedSamples.Max(y => y.CostLeft())).Diagnose();
            return;
        }

        Me.State = BotState.SamplesDiagnosed;
    }

    private static void OnArrivedAtMolecules()
    {
        // On a besoin de plus de 10 molecules. Pas besoin d'essayer, on enlève un sample.
        if (Me.NeedA() + Me.NeedB() + Me.NeedC() + Me.NeedD() + Me.NeedE() > 1 && Me.TotalStorage >= 9 && !Me.CarriedSamples.Any(x => x.CanBeProcessed()))
        {
            Console.Error.WriteLine("Removed samples, we do not have enough storage for all molecules");
            // Temporarily keep only one sample to try to finish it
            Me.CarriedSamples = new List<Sample> { Me.CarriedSamples.OrderBy(x => x.CostLeft()).First() };
        }

        if (Me.NeedMolecules() && Me.CanStoreMolecules())
        {
            if (Me.NeedA() > 0 && AvailableA > 0)
            {
                Write("Connect A");
                return;
            }

            if (Me.NeedB() > 0 && AvailableB > 0)
            {
                Write("Connect B");
                return;
            }

            if (Me.NeedC() > 0 && AvailableC > 0)
            {
                Write("Connect C");
                return;
            }

            if (Me.NeedD() > 0 && AvailableD > 0)
            {
                Write("Connect D");
                return;
            }

            if (Me.NeedE() > 0 && AvailableE > 0)
            {
                Write("Connect E");
                return;
            }

            // Wait if opponent is processing sample at the lab and we're waiting for molecules
            if (Opponent.Target == "LABORATORY" && Opponent.CarriedSamples.Any(x => x.CanBeProcessed()))
            {
                if (Opponent.CarriedSamples.Where(x => x.CanBeProcessed()).Any(x =>
                    Me.NeedA() > 0 && Me.NeedA() <= x.RealCostA(Opponent) ||
                    Me.NeedB() > 0 && Me.NeedB() <= x.RealCostB(Opponent) ||
                    Me.NeedC() > 0 && Me.NeedC() <= x.RealCostC(Opponent) ||
                    Me.NeedD() > 0 && Me.NeedD() <= x.RealCostD(Opponent) ||
                    Me.NeedE() > 0 && Me.NeedE() <= x.RealCostE(Opponent)
                ))
                {
                    Write("WAIT");
                    return;
                }
            }
        }

        ////Take molecules to fill storage
        //if (Me.CanStoreMolecules())
        //{
        //    Write($"Connect {Molecules.First(x => x.Available == Molecules.Where(m => m.CanTake()).Min(m => m.Available)).Type}");
        //    return;
        //}

        // Try to block opponent
        if (Me.CanStoreMolecules())
        {
            if (Opponent.Target != "MOLECULES" && Opponent.Eta > 0)
            {
                if (AvailableA == 1 && Opponent.NeedA() > 0)
                {
                    Console.Error.WriteLine("Block opponent by taking molecule");
                    Write("Connect A");
                    Me.State = BotState.MoleculesTaken;
                    return;
                }
                if (AvailableB == 1 && Opponent.NeedB() > 0)
                {
                    Console.Error.WriteLine("Block opponent by taking molecule");
                    Write("Connect B");
                    Me.State = BotState.MoleculesTaken;
                    return;
                }
                if (AvailableC == 1 && Opponent.NeedC() > 0)
                {
                    Console.Error.WriteLine("Block opponent by taking molecule");
                    Write("Connect C");
                    Me.State = BotState.MoleculesTaken;
                    return;
                }
                if (AvailableD == 1 && Opponent.NeedD() > 0)
                {
                    Console.Error.WriteLine("Block opponent by taking molecule");
                    Write("Connect D");
                    Me.State = BotState.MoleculesTaken;
                    return;
                }
                if (AvailableE == 1 && Opponent.NeedE() > 0)
                {
                    Console.Error.WriteLine("Block opponent by taking molecule");
                    Write("Connect E");
                    Me.State = BotState.MoleculesTaken;
                    return;
                }
            }
        }

        Me.State = BotState.MoleculesTaken;
    }

    private static void OnArrivedAtLaboratory()
    {
        var processeableSample = Me.CarriedSamples.FirstOrDefault(x => x.CanBeProcessed());
        if (processeableSample != null)
        {
            processeableSample.Process();
            return;
        }

        Me.State = BotState.ProcessDone;
    }

    private static void OnSamplesTaken()
    {
        if (Me.CarriedSamples.Any(x => x.State == SampleState.NonDiagnosed))
        {
            GotoDiagnosis();
            return;
        }

        if (!Me.CanStoreMolecules())
        {
            GotoDiagnosis(); // Will try to exchange a sample for another already in the cloud
            return;
        }

        Me.State = BotState.SamplesDiagnosed;
    }

    private static void OnSamplesDiagnosed()
    {
        if (Me.NeedMolecules() && Me.CanStoreMolecules())
        {
            GotoMolecules();
            return;
        }

        Me.State = BotState.MoleculesTaken;
    }

    private static void OnMoleculesTaken()
    {
        if (Me.CarriedSamples.Any(x => x.CanBeProcessed()))
        {
            GotoLaboratory();
            return;
        }

        Me.State = BotState.ProcessDone;
    }

    private static void OnProcessDone()
    {
        if (Me.NeedMolecules() && Me.CanStoreMolecules() && (
                Me.NeedA() > 0 && Me.NeedA() <= AvailableA ||
                Me.NeedB() > 0 && Me.NeedB() <= AvailableB ||
                Me.NeedC() > 0 && Me.NeedC() <= AvailableC ||
                Me.NeedD() > 0 && Me.NeedD() <= AvailableD ||
                Me.NeedE() > 0 && Me.NeedE() <= AvailableE))
            GotoMolecules();
        else if (Me.CarriedSamples.Count < 3)
            GotoSamples();
        else
            GotoMolecules();
    }

    public enum SampleState
    {
        Available,
        NonDiagnosed,
        Diagnosed
    }

    public class Sample
    {
        public Sample()
        {
            var inputs = Console.ReadLine().Split(' ');
            SampleId = int.Parse(inputs[0]);
            CarriedBy = int.Parse(inputs[1]);
            Rank = int.Parse(inputs[2]);
            ExpertiseGain = inputs[3];
            Health = int.Parse(inputs[4]);
            CostA = int.Parse(inputs[5]);
            CostB = int.Parse(inputs[6]);
            CostC = int.Parse(inputs[7]);
            CostD = int.Parse(inputs[8]);
            CostE = int.Parse(inputs[9]);

            if (CarriedBy == -1 && Health == -1)
                State = SampleState.Available;
            else if (Health == -1)
                State = SampleState.NonDiagnosed;
            else if (Health > -1)
                State = SampleState.Diagnosed;
        }

        public SampleState State { get; }

        public int SampleId { get; }
        public int CarriedBy { get; }
        public int Rank { get; }
        public string ExpertiseGain { get; }
        public int Health { get; }
        public int CostA { get; }
        public int CostB { get; }
        public int CostC { get; }
        public int CostD { get; }
        public int CostE { get; }

        public int MyCostA => RealCostA(Me);
        public int MyCostB => RealCostB(Me);
        public int MyCostC => RealCostC(Me);
        public int MyCostD => RealCostD(Me);
        public int MyCostE => RealCostE(Me);

        public bool IsTooCostly()
        {
            return MyCostA > 5 || MyCostB > 5 || MyCostC > 5 || MyCostD > 5 || MyCostE > 5;
        }

        public int RealCostA(Bot bot)
        {
            return Math.Max(CostA - bot.ExpertiseA, 0);
        }

        public int RealCostB(Bot bot)
        {
            return Math.Max(CostB - bot.ExpertiseB, 0);
        }

        public int RealCostC(Bot bot)
        {
            return Math.Max(CostC - bot.ExpertiseC, 0);
        }

        public int RealCostD(Bot bot)
        {
            return Math.Max(CostD - bot.ExpertiseD, 0);
        }

        public int RealCostE(Bot bot)
        {
            return Math.Max(CostE - bot.ExpertiseE, 0);
        }

        public void Diagnose()
        {
            Write($"CONNECT {SampleId}");
        }

        public bool CanBeProcessed()
        {
            return Me.StorageA >= MyCostA && Me.StorageB >= MyCostB && Me.StorageC >= MyCostC && Me.StorageD >= MyCostD &&
                   Me.StorageE >= MyCostE;
        }

        public int CostLeft()
        {
            return Math.Max(MyCostA - Me.StorageA, 0) +
                   Math.Max(MyCostB - Me.StorageB, 0) +
                   Math.Max(MyCostC - Me.StorageC, 0) +
                   Math.Max(MyCostD - Me.StorageD, 0) +
                   Math.Max(MyCostE - Me.StorageE, 0);
        }

        public void Process()
        {
            Write($"CONNECT {SampleId}");
            Me.CarriedSamples.Remove(this);
        }
    }

    public class Molecule
    {
        public Molecule(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public int Available { get; set; }

        public bool CanTake()
        {
            return Available > 0;
        }
    }
}