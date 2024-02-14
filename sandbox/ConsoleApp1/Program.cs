using Microsoft.Extensions.Time.Testing;
using R3;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;


SynchronizationContext.SetSynchronizationContext(new MySyncContext());


var channel = ChannelUtility.CreateSingleReadeWriterUnbounded<int>();

//var t = Foo();

// Task.Run(() => 10).ConfigureAwait(ConfigureAwaitOptions.None);
//Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

//channel.Writer.TryWrite(100);


//t.Wait();
var timeProvider = new FakeTimeProvider();

var subject = new Subject<int>();

subject
    .Do(x => Console.WriteLine($"Do:{Thread.CurrentThread.ManagedThreadId}"))
    .SubscribeAwait(async (_, ct) =>
    {
        Console.WriteLine($"Before Await:{Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, ct);
        Console.WriteLine($"After Yield:{Thread.CurrentThread.ManagedThreadId}");
    }, AwaitOperation.Sequential/*, configureAwait: false*/);


subject.OnNext(10);
subject.OnNext(20);
subject.OnNext(30);

timeProvider.Advance(TimeSpan.FromSeconds(1));
Console.ReadLine();





internal static class ChannelUtility
{
    static readonly UnboundedChannelOptions options = new UnboundedChannelOptions
    {
        SingleWriter = true, // in Rx operator, OnNext gurantees synchronous
        SingleReader = true, // almostly uses single reader loop
        AllowSynchronousContinuations = true // if false, uses TaskCreationOptions.RunContinuationsAsynchronously so avoid it.
    };

    internal static Channel<T> CreateSingleReadeWriterUnbounded<T>()
    {
        return Channel.CreateUnbounded<T>(options);
    }
}


public sealed class ClampedReactiveProperty<T>(T initialValue, T min, T max)
    : ReactiveProperty<T>(initialValue) where T : IComparable<T>
{
    private static IComparer<T> Comparer { get; } = Comparer<T>.Default;

    protected override void OnValueChanging(ref T value)
    {
        if (Comparer.Compare(value, min) < 0)
        {
            value = min;
        }
        else if (Comparer.Compare(value, max) > 0)
        {
            value = max;
        }
    }
}


public sealed class ClampedReactiveProperty2<T>
    : ReactiveProperty<T> where T : IComparable<T>
{
    private static IComparer<T> Comparer { get; } = Comparer<T>.Default;

    readonly T min, max;

    // callOnValueChangeInBaseConstructor to avoid OnValueChanging call before min, max set.
    public ClampedReactiveProperty2(T initialValue, T min, T max)
        : base(initialValue, EqualityComparer<T>.Default, callOnValueChangeInBaseConstructor: false)
    {
        this.min = min;
        this.max = max;

        // modify currentValue manually
        OnValueChanging(ref GetValueRef());
    }

    protected override void OnValueChanging(ref T value)
    {
        if (Comparer.Compare(value, min) < 0)
        {
            value = min;
        }
        else if (Comparer.Compare(value, max) > 0)
        {
            value = max;
        }
    }
}





//[JsonConverter(typeof(IgnoreCaseStringReactivePropertyJsonConverter))]
public class IgnoreCaseStringReactiveProperty : ReactiveProperty<string>
{
    public IgnoreCaseStringReactiveProperty(string value)
        : base(value, StringComparer.OrdinalIgnoreCase)
    {

    }
}

internal class IgnoreCaseStringReactivePropertyJsonConverter : ReactivePropertyJsonConverter<string>
{
    protected override ReactiveProperty<string> CreateReactiveProperty(string value)
    {
        return new IgnoreCaseStringReactiveProperty(value);
    }
}

public class MySyncContext : SynchronizationContext
{
    public MySyncContext()
    {
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        base.Post(d, state);
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        base.Send(d, state);
    }
}
