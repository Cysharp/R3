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
    const byte NotCompleted = 0;
    const byte CompletedSuccess = 1;
    const byte CompletedFailure = 2;
    const byte Disposed = 3;

    // Memory Size: 1(byte) + 8(IntPtr) + sizeof(T) + 8(IntPtr) and subscriptions(nodes).
    byte completeState;
    Exception? error;
    T currentValue;
    IEqualityComparer<T>? equalityComparer; // allow null for no-compare

    // For reduce memory usage, ReactiveProperty<T> itself is LinkedList and subscription represents LinkedListNode.
    // The last of node is root.Previous(if null, single list).
    ObserverNode? root;

    public IEqualityComparer<T>? EqualityComparer => equalityComparer;

    public override T CurrentValue => currentValue;

    public bool HasObservers => root != null;
    public bool IsCompleted => completeState == CompletedSuccess || completeState == CompletedFailure;
    public bool IsDisposed => completeState == Disposed;
    public bool IsCompletedOrDisposed => IsCompleted || IsDisposed;

    public virtual T Value
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

    public virtual void ForceNotify()
    {
        OnNext(Value);
    }

    public virtual void OnNext(T value)
    {
        OnValueChanging(ref value);
        this.currentValue = value; // different from Subject<T>; set value before raise OnNext
        OnValueChanged(value);  // for inheritance types.

        OnNextCore(value);
    }

    protected virtual void OnNextCore(T value)
    {
        ThrowIfDisposed();
        if (IsCompleted) return;

        var node = root;
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
        ThrowIfDisposed();
        if (IsCompleted) return;

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
        ThrowIfDisposed();
        if (IsCompleted) return;

        ObserverNode? node = null;
        lock (this) // I know lock(this) is dangerous.
        {
            if (completeState == NotCompleted)
            {
                completeState = result.IsSuccess ? CompletedSuccess : CompletedFailure;
                error = result.Exception;
                node = Volatile.Read(ref root);
                Volatile.Write(ref root, null); // when complete, List is clear.
            }
            else
            {
                // IsCompleted = do-nothing, IsDisposed = throw
                ThrowIfDisposed();
                return;
            }
        }

        if (result.IsFailure)
        {
            OnReceiveError(result.Exception);
        }

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
        Result? completedResult;
        lock (this)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                completedResult = (error == null) ? Result.Success : Result.Failure(error);
            }
            else
            {
                completedResult = null;
            }
        }

        if (completedResult != null)
        {
            goto PUBLISH_CURRENT_AND_RESULT;
        }

        // raise latest value on subscribe(before add observer to list)
        observer.OnNext(currentValue);

        lock (this)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                completedResult = (error == null) ? Result.Success : Result.Failure(error);
                goto PUBLISH_RESULT;
            }

            // create subscription and add to list in lock.
            var subscription = new ObserverNode(this, observer);
            return subscription;
        }

    PUBLISH_CURRENT_AND_RESULT:
        if (completedResult != null)
        {
            if (completedResult.Value.IsSuccess)
            {
                observer.OnNext(currentValue);
            }
            observer.OnCompleted(completedResult.Value);
            return Disposable.Empty;
        }

    PUBLISH_RESULT:
        if (completedResult != null)
        {
            observer.OnCompleted(completedResult.Value);
        }
        return Disposable.Empty;
    }

    void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException("");
    }

    public override void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool callOnCompleted)
    {
        ObserverNode? node = null;
        lock (this)
        {
            if (completeState == Disposed)
            {
                return;
            }

            // not yet disposed so can call list iteration
            if (callOnCompleted && !IsCompleted)
            {
                node = Volatile.Read(ref root);
            }

            Volatile.Write(ref root, null);
            completeState = Disposed;
        }

        while (node != null)
        {
            node.Observer.OnCompleted();
            node = node.Next;
        }

        DisposeCore();
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

    ObserverNode[] Nodes
    {
        get
        {
            var list = new List<ObserverNode>();
            var node = root;
            while (node != null)
            {
                list.Add(node);
                node = node.Next;
            }
            return list.ToArray();
        }
    }

#endif

    sealed class ObserverNode : IDisposable
    {
        public readonly Observer<T> Observer;

        ReactiveProperty<T>? parent;

        public ObserverNode? Previous { get; set; } // Previous is last node or root(null).
        public ObserverNode? Next { get; set; }


        // for debugging field/property
#if DEBUG
        static int idGenerator;
        public int Id = Interlocked.Increment(ref idGenerator);
        public bool IsRootNode => parent?.root == this;
        public bool IsSingleRootNode => IsRootNode && Previous == null;

        public override string ToString()
        {
            return $"{Previous?.Id} -> ({Id}) -> {Next?.Id}";
        }
#endif

        public ObserverNode(ReactiveProperty<T> parent, Observer<T> observer)
        {
            this.parent = parent;
            this.Observer = observer;

            // Add node(self) to list(ReactiveProperty), called in lock
            if (parent.root == null)
            {
                // Single list(both previous and next is null)
                Volatile.Write(ref parent.root, this);
            }
            else
            {
                // previous is last, null then root is last.
                var lastNode = parent.root.Previous ?? parent.root;

                lastNode.Next = this;
                this.Previous = lastNode;
                parent.root.Previous = this;
            }
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            // keep this.Next for dispose on iterating
            // Remove node(self) from list(ReactiveProperty)
            lock (p)
            {
                if (p.IsCompletedOrDisposed) return;

                if (this == p.root)
                {
                    if (this.Previous == null || this.Next == null)
                    {
                        // case of single list
                        p.root = null;
                    }
                    else
                    {
                        // otherwise, root is next node.
                        var root = this.Next;

                        // single list.
                        if (root.Next == null)
                        {
                            root.Previous = null;
                        }
                        else
                        {
                            root.Previous = this.Previous; // as last.
                        }

                        p.root = root;
                    }
                }
                else
                {
                    // node is not root, previous must exists
                    this.Previous!.Next = this.Next;
                    if (this.Next != null)
                    {
                        this.Next.Previous = this.Previous;
                    }
                    else
                    {
                        // next does not exists, previous is last node so modify root
                        p.root!.Previous = this.Previous;
                    }
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

// for SynchronizedReactiveProperty

internal class SynchronizedReactivePropertyJsonConverterFactory : ReactivePropertyJsonConverterFactory
{
    protected override Type GenericConverterType => typeof(SynchronizedReactivePropertyJsonConverter<>);
}

internal class SynchronizedReactivePropertyJsonConverter<T> : ReactivePropertyJsonConverter<T>
{
    protected override ReactiveProperty<T> CreateReactiveProperty(T value)
    {
        return new SynchronizedReactiveProperty<T>(value);
    }
}

#endif
