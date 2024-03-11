using Microsoft.Extensions.Time.Testing;
using R3;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;


var p1 = new ReactiveProperty<int>();

Console.WriteLine(p1.HasObservers);
var d = p1.Skip(1).Subscribe(x => Debug.Log("[P1]" + x));



var d1 = p1.Skip(1).Subscribe(x => Debug.Log("[P2]" + x));
var d2 = p1.Skip(1).Subscribe(x => Debug.Log("[P2]" + x));
d1.Dispose();
d2.Dispose();

var d3 = p1.Skip(1).Subscribe(x => Debug.Log("[P3]" + x)); // P3 is not raised

Console.WriteLine(p1.HasObservers);

p1.Value = 1;
p1.Value = 2;

d.Dispose();

Console.WriteLine(p1.HasObservers);

d3.Dispose();

Console.WriteLine(p1.HasObservers);


public static class Debug
{
    public static void Log(string x)
    {
        Console.WriteLine(x);
    }
}

public class Person : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    string name = default!;

    public required string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
        }
    }
}


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


class NewsViewModel
{
    ReactiveProperty<NewsUiState> _uiState = new(new NewsUiState());
    public ReadOnlyReactiveProperty<NewsUiState> UiState => _uiState;
}

class NewsUiState
{
}

