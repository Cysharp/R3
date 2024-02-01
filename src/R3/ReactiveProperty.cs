#if NET6_0_OR_GREATER
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace R3;

public abstract class ReadOnlyReactiveProperty<T> : Observable<T>, IDisposable
{
    public abstract T CurrentValue { get; }
    protected virtual void OnSetValue(T value) { }
    protected virtual void OnReceiveError(Exception exception) { }
    public ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty() => this;
    public abstract void Dispose();
}

// almostly same code as Subject<T>.

// allow inherit

#if NET6_0_OR_GREATER
[JsonConverter(typeof(ReactivePropertyJsonConverterFactory))]
#endif
public class ReactiveProperty<T> : ReadOnlyReactiveProperty<T>, ISubject<T>
{
    T currentValue;
    IEqualityComparer<T>? equalityComparer;
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)

    public IEqualityComparer<T>? EqualityComparer => equalityComparer;

    public override T CurrentValue => currentValue;

    public bool IsDisposed => completeState.IsDisposed;

    public T Value
    {
        get => this.currentValue;
        set
        {
            if (EqualityComparer != null)
            {
                if (EqualityComparer.Equals(this.currentValue, value))
                {
                    return;
                }
            }

            OnNext(value);
        }
    }

    public ReactiveProperty() : this(default!)
    {
    }

    public ReactiveProperty(T value)
        : this(value, EqualityComparer<T>.Default)
    {
    }

    public ReactiveProperty(T value, IEqualityComparer<T>? equalityComparer)
    {
        this.currentValue = value;
        this.equalityComparer = equalityComparer;
        this.list = new FreeListCore<Subscription>(this); // use self as gate(reduce memory usage), this is slightly dangerous so don't lock this in user.
    }

    public void OnNext(T value)
    {
        if (completeState.IsCompleted) return;

        this.currentValue = value; // different from Subject<T>; set value before raise OnNext
        OnSetValue(value);  // for inheritance types.

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnNext(value);
        }
    }

    public void OnErrorResume(Exception error)
    {
        if (completeState.IsCompleted) return;

        OnReceiveError(error);

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnErrorResume(error);
        }
    }

    public void OnCompleted(Result result)
    {
        var status = completeState.TrySetResult(result);
        if (status != CompleteState.ResultStatus.Done)
        {
            return; // already completed
        }

        if (result.IsFailure)
        {
            OnReceiveError(result.Exception);
        }

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnCompleted(result);
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var result = completeState.TryGetResult();
        if (result != null)
        {
            observer.OnNext(currentValue);
            observer.OnCompleted(result.Value);
            return Disposable.Empty;
        }

        // raise latest value on subscribe(before add observer to list)
        observer.OnNext(currentValue);

        var subscription = new Subscription(this, observer); // create subscription and add observer to list.

        // need to check called completed during adding
        result = completeState.TryGetResult();
        if (result != null)
        {
            subscription.observer.OnCompleted(result.Value);
            subscription.Dispose();
            return Disposable.Empty;
        }

        return subscription;
    }

    public override void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool callOnCompleted)
    {
        if (completeState.TrySetDisposed(out var alreadyCompleted))
        {
            if (callOnCompleted && !alreadyCompleted)
            {
                // not yet disposed so can call list iteration
                foreach (var subscription in list.AsSpan())
                {
                    subscription?.observer.OnCompleted();
                }
            }

            DisposeCore();
            list.Dispose();
        }
    }

    protected virtual void DisposeCore() { }

    public override string? ToString()
    {
        return (currentValue == null) ? "(null)" : currentValue.ToString();
    }

    sealed class Subscription : IDisposable
    {
        public readonly Observer<T> observer;
        readonly int removeKey;
        ReactiveProperty<T>? parent;

        public Subscription(ReactiveProperty<T> parent, Observer<T> observer)
        {
            this.parent = parent;
            this.observer = observer;
            parent.list.Add(this, out removeKey); // for the thread-safety, add and set removeKey in same lock.
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            // removeKey is index, will reuse if remove completed so only allows to call from here and must not call twice.
            p.list.Remove(removeKey);
        }
    }
}

#if NET6_0_OR_GREATER

public class ReactivePropertyJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return GetReactivePropertyType(typeToConvert) != null;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var t = GetReactivePropertyType(typeToConvert);
        if (t != null)
        {
            var rt = GenericConverterType.MakeGenericType(t.GetGenericArguments()[0]);
            return (JsonConverter?)Activator.CreateInstance(rt, false);
        }
        else
        {
            return null;
        }
    }

    Type? GetReactivePropertyType(Type? type)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
            {
                return type;
            }

            type = type.BaseType;
        }
        return null;
    }

    // allow customize
    protected virtual Type GenericConverterType => typeof(ReactivePropertyJsonConverter<>);
}

public class ReactivePropertyJsonConverter<T> : JsonConverter<ReactiveProperty<T>>
{
    public override void Write(Utf8JsonWriter writer, ReactiveProperty<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }

    public override ReactiveProperty<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var v = JsonSerializer.Deserialize<T>(ref reader, options);
        return CreateReactiveProperty(v!);
    }

    // allow customize
    protected virtual ReactiveProperty<T> CreateReactiveProperty(T value)
    {
        return new ReactiveProperty<T>(value);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return GetReactivePropertyType(typeToConvert) != null;
    }

    Type? GetReactivePropertyType(Type? type)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
            {
                return type;
            }

            type = type.BaseType;
        }
        return null;
    }
}

// for BindableReactiveProperty

internal class BindableReactivePropertyJsonConverterFactory : ReactivePropertyJsonConverterFactory
{
    protected override Type GenericConverterType => typeof(BindableReactivePropertyJsonConverter<>);
}

internal class BindableReactivePropertyJsonConverter<T> : ReactivePropertyJsonConverter<T>
{
    protected override ReactiveProperty<T> CreateReactiveProperty(T value)
    {
        return new BindableReactiveProperty<T>(value);
    }
}


#endif
