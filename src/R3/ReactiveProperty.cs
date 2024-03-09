#if NET6_0_OR_GREATER
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace R3;

public abstract class ReadOnlyReactiveProperty<T> : Observable<T>, IDisposable
{
    public abstract T CurrentValue { get; }
    protected virtual void OnValueChanged(T value) { }
    protected virtual void OnReceiveError(Exception exception) { }
    public ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty() => this;
    public abstract void Dispose();
}

// allow inherit

#if NET6_0_OR_GREATER
[JsonConverter(typeof(ReactivePropertyJsonConverterFactory))]
#endif
public class ReactiveProperty<T> : ReadOnlyReactiveProperty<T>, ISubject<T>
{
    T currentValue;
    IEqualityComparer<T>? equalityComparer;
    ObserverNode? root; // Root of LinkedList Node(Root.Previous is Last)
    CompleteState completeState;     // struct(int, IntPtr)

    public IEqualityComparer<T>? EqualityComparer => equalityComparer;

    public override T CurrentValue => currentValue;

    public bool IsDisposed => completeState.IsDisposed;

    public T Value
    {
        get => this.currentValue;
        set
        {
            OnValueChanging(ref value);

            if (EqualityComparer != null)
            {
                if (EqualityComparer.Equals(this.currentValue, value))
                {
                    return;
                }
            }

            this.currentValue = value;
            OnValueChanged(value);

            OnNextCore(value);
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
        this.equalityComparer = equalityComparer;
        OnValueChanging(ref value);
        this.currentValue = value;
        OnValueChanged(value);
    }

    protected ReactiveProperty(T value, IEqualityComparer<T>? equalityComparer, bool callOnValueChangeInBaseConstructor)
    {
        this.equalityComparer = equalityComparer;

        if (callOnValueChangeInBaseConstructor)
        {
            OnValueChanging(ref value);
        }

        this.currentValue = value;

        if (callOnValueChangeInBaseConstructor)
        {
            OnValueChanged(value);
        }
    }

    protected virtual void OnValueChanging(ref T value) { }
    protected ref T GetValueRef() => ref currentValue; // dangerous

    public void ForceNotify()
    {
        OnNext(Value);
    }

    public void OnNext(T value)
    {
        OnValueChanging(ref value);
        this.currentValue = value; // different from Subject<T>; set value before raise OnNext
        OnValueChanged(value);  // for inheritance types.

        OnNextCore(value);
    }

    void OnNextCore(T value)
    {
        if (completeState.IsCompleted) return;

        var node = Volatile.Read(ref root);
        var last = node?.Previous;
        while (node != null)
        {
            node.Observer.OnNext(value);
            if (node == last) return;
            node = node.Next;
        }
    }

    public void OnErrorResume(Exception error)
    {
        if (completeState.IsCompleted) return;

        OnReceiveError(error);

        var node = Volatile.Read(ref root);
        var last = node?.Previous;
        while (node != null)
        {
            node.Observer.OnErrorResume(error);
            if (node == last) return;
            node = node.Next;
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

        var node = Volatile.Read(ref root);
        var last = node?.Previous;
        while (node != null)
        {
            node.Observer.OnCompleted(result);
            if (node == last) return;
            node = node.Next;
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

        var subscription = new ObserverNode(this, observer); // create subscription

        // need to check called completed during adding
        result = completeState.TryGetResult();
        if (result != null)
        {
            subscription.Observer.OnCompleted(result.Value);
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
                var node = Volatile.Read(ref root);
                while (node != null)
                {
                    node.Observer.OnCompleted();
                    node = node.Next;
                }
            }

            DisposeCore();
            Volatile.Write(ref root, null);
        }
    }

    protected virtual void DisposeCore() { }

    public override string? ToString()
    {
        return (currentValue == null) ? "(null)" : currentValue.ToString();
    }

    // debugging property

#if DEBUG

    public int NodeCount
    {
        get
        {
            var count = 0;
            var node = root;
            while (node != null)
            {
                count++;
                node = node.Next;
            }
            return count;
        }
    }

#endif

    sealed class ObserverNode : IDisposable
    {
        public readonly Observer<T> Observer;

        ReactiveProperty<T>? parent;

        public ObserverNode Previous { get; set; }
        public ObserverNode? Next { get; set; }


        // for debugging property
#if DEBUG
        public bool IsRootNode => parent?.root == this;
        public bool IsSingleRootNode => IsRootNode && Previous == this;
        public bool HasNext => Previous == this;
#endif

        public ObserverNode(ReactiveProperty<T> parent, Observer<T> observer)
        {
            this.parent = parent;
            this.Observer = observer;

            if (parent.root == null)
            {
                Volatile.Write(ref parent.root, this);
                this.Previous = this;
            }
            else
            {
                var last = parent.root.Previous;
                last.Next = this;
                this.Previous = last;
                parent.root.Previous = this; // this as last
            }
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            // keep this.Next for dispose on iterating

            if (this.Previous == this) // single list
            {
                p.root = null;
                return;
            }

            if (p.root == this)
            {
                var next = this.Next;
                p.root = next;
                if (next != null)
                {
                    if (next.Next != null)
                    {
                        next.Previous = this.Previous;
                    }
                    else
                    {
                        next.Previous = next;
                    }
                }
            }
            else
            {
                var prev = this.Previous;
                var next = this.Next;
                prev.Next = next;
                if (next != null)
                {
                    if (next.Next != null)
                    {
                        next.Previous = prev;
                    }
                }

                // root is single node
                if (p.root != null && p.root.Next == null)
                {
                    p.root.Previous = p.root;
                }
            }
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
