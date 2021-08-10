using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace TestApp.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }

    [SimpleJob(RuntimeMoniker.Net50)]
    public class ParsingBench
    {
        [Params("*.9.*/2 1-5 10:00:00.000", "*.4.6,7 * *:*:*.1,2,3-5,10-20/3", "2100.12.31 23:59:59.999",
            "*.*.1 0:0:0")]
        public string Pattern = null!;

        [Benchmark(Baseline = true)]
        public object ParsingOld()
        {
            return new Schedule(Pattern);
        }

        [Benchmark]
        public object ParsingNew()
        {
            return new NewSchedule(Pattern);
        }
    }

    [SimpleJob(RuntimeMoniker.Net50)]
    public class FindNextBench
    {
        [Params("*.9.*/2 1-5 10:00:00.000", "*.4.6,7 * *:*:*.1,2,3-5,10-20/3", "2100.12.31 23:59:59.999",
            "*.*.1 0:0:0")]
        public string Pattern = null!;

        [Params("2001-01-01", "2080-05-05")] public string DateString = null!;

        private DateTime Date;
        private Schedule OldSchedule = null!;
        private NewSchedule NewSchedule = null!;
        private NewScheduleCopypasted NewScheduleCopypasted = null!;
        private TreeSchedule TreeSchedule = null!;

        [GlobalSetup]
        public void Setup()
        {
            Date = DateTime.ParseExact(DateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            OldSchedule = new Schedule(Pattern);
            NewSchedule = new NewSchedule(Pattern);
            NewScheduleCopypasted = new NewScheduleCopypasted(Pattern);
            TreeSchedule = new TreeSchedule(Pattern);
        }

        [Benchmark(Baseline = true)]
        public object FindNextOld() => OldSchedule.NextEvent(Date);

        [Benchmark]
        public object FindNextNewCopypasted() => NewScheduleCopypasted.NextEvent(Date);

        [Benchmark]
        public object FindNextNewTypelevel() => NewSchedule.NextEvent(Date);
        
        [Benchmark]
        public object FindNextTree() => TreeSchedule.NextEvent(Date);
    }

    [SimpleJob(RuntimeMoniker.Net50)]
    public class FindPrevBench
    {
        [Params("*.9.*/2 1-5 10:00:00.000", "*.4.6,7 * *:*:*.1,2,3-5,10-20/3", "2000.12.31 23:59:59.999",
            "*.*.1 0:0:0")]
        public string Pattern = null!;

        [Params("2001-01-01", "2080-05-05")] public string DateString = null!;

        private DateTime Date;
        private Schedule OldSchedule = null!;
        private NewSchedule NewSchedule = null!;
        private NewScheduleCopypasted NewScheduleCopypasted = null!;

        [GlobalSetup]
        public void Setup()
        {
            Date = DateTime.ParseExact(DateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            OldSchedule = new Schedule(Pattern);
            NewSchedule = new NewSchedule(Pattern);
            NewScheduleCopypasted = new NewScheduleCopypasted(Pattern);
        }

        [Benchmark(Baseline = true)]
        public object FindPrevOld() => OldSchedule.PrevEvent(Date);

        [Benchmark]
        public object FindPrevNewCopypasted() => NewScheduleCopypasted.PrevEvent(Date);

        [Benchmark]
        public object FindPrevNewTypelevel() => NewSchedule.PrevEvent(Date);
    }
}