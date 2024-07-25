using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using R3;
using System.Reactive.Concurrency;
using System.Reflection;

BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args);

file class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddDiagnoser(MemoryDiagnoser.Default);
        AddJob(Job.ShortRun.WithWarmupCount(1).WithIterationCount(1));
    }
}

[Config(typeof(BenchmarkConfig))]
public class RangeIterate
{
    [Benchmark]
    public IDisposable R3()
    {
        return global::R3.Observable.Range(1, 10000).Subscribe();
    }

    [Benchmark]
    public IDisposable DotnetReactive_Immediate()
    {
        return System.Reactive.Linq.Observable.Range(1, 10000, Scheduler.Immediate).Subscribe();
    }

    [Benchmark]
    public IDisposable DotnetReactive_CurrentThread()
    {
        return System.Reactive.Linq.Observable.Range(1, 10000, Scheduler.CurrentThread).Subscribe();
    }
}

[Config(typeof(BenchmarkConfig))]
public class SubjectSubscribeDispose
{
    const int C = 10000;

    IDisposable[] disposables = default!;

    [IterationSetup]
    public void Setup()
    {
        disposables = new IDisposable[C];
    }

    [Benchmark]
    public void R3()
    {
        var subject = new global::R3.Subject<int>();
        for (int i = 0; i < C; i++)
        {
            disposables[i] = subject.Subscribe();
        }
        for (int i = 0; i < C; i++)
        {
            disposables[i].Dispose();
        }
    }

    [Benchmark]
    public void DotnetReactive()
    {
        var subject = new System.Reactive.Subjects.Subject<int>();
        for (int i = 0; i < C; i++)
        {
            disposables[i] = subject.Subscribe();
        }
        for (int i = 0; i < C; i++)
        {
            disposables[i].Dispose();
        }
    }
}



//[Config(typeof(BenchmarkConfig))]
//public class SubjectVsSlim
//{
//    const int C = 100000;

//    Subject<int> subject = new();
//    SubjectSlim<int> subjectSlim = new();

//    [IterationSetup]
//    public void Setup()
//    {
//        for (int i = 0; i < C; i++)
//        {
//            subject.Subscribe();
//        }
//        for (int i = 0; i < C; i++)
//        {
//            subjectSlim.Subscribe();
//        }
//    }

//    [Benchmark]
//    public void Subject()
//    {
//        subject.OnNext(1);
//    }

//    [Benchmark]
//    public void SubjectSlim()
//    {
//        subjectSlim.OnNext(1);
//    }
//}


//[Config(typeof(BenchmarkConfig))]
//public class SubjectVsSlim2
//{
//    const int C = 100000;

//    IDisposable[] disposables = default!;

//    [IterationSetup]
//    public void Setup()
//    {
//        disposables = new IDisposable[C];
//    }

//    [Benchmark]
//    public void Subject()
//    {
//        var subject = new global::R3.Subject<int>();
//        for (int i = 0; i < C; i++)
//        {
//            disposables[i] = subject.Subscribe();
//        }
//        for (int i = 0; i < C; i++)
//        {
//            disposables[i].Dispose();
//        }
//    }

//    [Benchmark]
//    public void SubjectSlim()
//    {
//        var subject = new global::R3.SubjectSlim<int>();
//        for (int i = 0; i < C; i++)
//        {
//            disposables[i] = subject.Subscribe();
//        }
//        for (int i = 0; i < C; i++)
//        {
//            disposables[i].Dispose();
//        }
//    }
//}
