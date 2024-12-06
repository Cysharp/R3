using System.Runtime.CompilerServices;
using System.Windows.Input; // for XAML binding

namespace R3;

public class ReactiveCommand<T> : Observable<T>, ICommand, IDisposable
{
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)
    IDisposable subscription; // from canExecuteSource (and onNext).
    bool canExecute; // set from observable sequence
    int executionCount;
    readonly object gate = new();

    public event EventHandler? CanExecuteChanged;

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveCommand()
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = true;
        this.subscription = Disposable.Empty;
    }

    public ReactiveCommand(Action<T> execute)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = true;
        this.subscription = this.Subscribe((HandleExecutionAction: (Action<T, Action<T>>)HandleExecution, Action: execute), static (value, state) => state.HandleExecutionAction(value, state.Action));
    }

    public ReactiveCommand(Func<T, CancellationToken, ValueTask> executeAsync, AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true, bool cancelOnCompleted = false, int maxSequential = -1)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = true;
        this.subscription = this.SubscribeAwait((HandleAsyncExecutionFunc: (Func<T, Func<T, CancellationToken, ValueTask>, CancellationToken, ValueTask>)HandleAsyncExecution, Func: executeAsync), static (value, state, cancellationToken) => state.HandleAsyncExecutionFunc(value, state.Func, cancellationToken), awaitOperation, configureAwait, cancelOnCompleted, maxSequential);
    }

    public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = initialCanExecute;
        this.subscription = canExecuteSource.Subscribe(this, static (newCanExecute, state) =>
        {
            state.ChangeCanExecute(newCanExecute);
        });
    }

    bool ICommand.CanExecute(object? _) // parameter is ignored
    {
        return CanExecute();
    }

    void ICommand.Execute(object? parameter)
    {
        if (typeof(T) == typeof(Unit))
        {
            Execute(Unsafe.As<Unit, T>(ref Unsafe.AsRef(in Unit.Default)));
        }
        else
        {
            Execute((T)parameter!);
        }
    }

    public void ChangeCanExecute(bool canExecute)
    {
        if (this.canExecute == canExecute) return;
        this.canExecute = canExecute;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDisabled => !CanExecute();

    public bool CanExecute()
    {
        return canExecute;
    }

    public void Execute(T parameter)
    {
        if (completeState.IsCompleted) return;

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnNext(parameter);
        }
    }

    internal void CombineSubscription(IDisposable disposable)
    {
        this.subscription = Disposable.Combine(this.subscription, disposable);
    }

    private void HandleExecution<T>(T value, Action<T> execute)
    {
        try
        {
            lock (gate)
            {
                executionCount++;
                IsExecuting.Value = executionCount > 0;
            }

            execute(value);
        }
        finally
        {
            lock (gate)
            {
                executionCount--;
                if (executionCount == 0)
                {
                    IsExecuting.Value = false;
                }
            }
        }
    }

    private async ValueTask HandleAsyncExecution<T>(T value, Func<T, CancellationToken, ValueTask> executeAsync, CancellationToken cancellationToken)
    {
        try
        {
            lock (gate)
            {
                executionCount++;
                IsExecuting.Value = executionCount > 0;
            }

            await executeAsync(value, cancellationToken);
        }
        finally
        {
            lock (gate)
            {
                executionCount--;
                if (executionCount == 0)
                {
                    IsExecuting.Value = false;
                }
            }
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var result = completeState.TryGetResult();
        if (result != null)
        {
            observer.OnCompleted(result.Value);
            return Disposable.Empty;
        }

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

    public void Dispose()
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

            IsExecuting.Dispose();
            list.Dispose();
            subscription.Dispose();
        }
    }


    sealed class Subscription : IDisposable
    {
        public readonly Observer<T> observer;
        readonly int removeKey;
        ReactiveCommand<T>? parent;

        public Subscription(ReactiveCommand<T> parent, Observer<T> observer)
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

public class ReactiveCommand<TInput, TOutput> : Observable<TOutput>, ICommand, IDisposable
{
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)
    bool canExecute; // set from observable sequence
    IDisposable subscription;
    int executionCount;
    readonly object gate = new();

    readonly Func<TInput, TOutput>? convert;     // for sync
    SingleAssignmentSubject<TInput>? asyncInput; // for async

    public event EventHandler? CanExecuteChanged;

    public ReactiveProperty<bool> IsExecuting { get; } = new();

    public ReactiveCommand(Func<TInput, TOutput> convert)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = true;
        this.convert = convert;
        this.subscription = Disposable.Empty;
    }

    public ReactiveCommand(Func<TInput, CancellationToken, ValueTask<TOutput>> convertAsync, AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true, bool cancelOnCompleted = false, int maxSequential = -1)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = true;
        this.asyncInput = new SingleAssignmentSubject<TInput>();
        this.subscription = asyncInput.SelectAwait((Command: this, ConvertAsync: convertAsync), static (value, state, cancellationToken) => state.Command.HandleAsyncExecution(value, state.ConvertAsync, cancellationToken), awaitOperation, configureAwait, cancelOnCompleted, maxSequential).Subscribe(this, static (x, state) =>
        {
            if (state.completeState.IsCompleted) return;

            foreach (var subscription in state.list.AsSpan())
            {
                subscription?.observer.OnNext(x);
            }
        });
    }

    public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute, Func<TInput, TOutput> convert)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = initialCanExecute;
        this.convert = convert;
        this.subscription = canExecuteSource.Subscribe(this, static (newCanExecute, state) =>
        {
            state.ChangeCanExecute(newCanExecute);
        });
    }

    public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute, Func<TInput, CancellationToken, ValueTask<TOutput>> convertAsync, AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true, bool cancelOnCompleted = false, int maxSequential = -1)
    {
        this.list = new FreeListCore<Subscription>(this);
        this.canExecute = initialCanExecute;
        var subscription1 = canExecuteSource.Subscribe(this, static (newCanExecute, state) =>
        {
            state.ChangeCanExecute(newCanExecute);
        });

        this.asyncInput = new SingleAssignmentSubject<TInput>();
        var subscription2 = asyncInput.SelectAwait(convertAsync, awaitOperation, configureAwait, cancelOnCompleted, maxSequential).Subscribe(this, static (x, state) =>
        {
            if (state.completeState.IsCompleted) return;

            foreach (var subscription in state.list.AsSpan())
            {
                subscription?.observer.OnNext(x);
            }
        });

        this.subscription = Disposable.Combine(subscription1, subscription2);
    }

    bool ICommand.CanExecute(object? _) // parameter is ignored
    {
        return CanExecute();
    }

    void ICommand.Execute(object? parameter)
    {
        if (typeof(TInput) == typeof(Unit))
        {
            Execute(Unsafe.As<Unit, TInput>(ref Unsafe.AsRef(in Unit.Default)));
        }
        else
        {
            Execute((TInput)parameter!);
        }
    }

    public void ChangeCanExecute(bool canExecute)
    {
        if (this.canExecute == canExecute) return;
        this.canExecute = canExecute;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDisabled => !CanExecute();

    public bool CanExecute()
    {
        return canExecute;
    }

    public void Execute(TInput parameter)
    {
        if (completeState.IsCompleted) return;

        if (convert != null)
        {
            // sync
            foreach (var subscription in list.AsSpan())
            {
                subscription?.observer.OnNext(convert(parameter));
            }
        }
        else if (asyncInput != null)
        {
            // async
            asyncInput.OnNext(parameter);
        }
    }

    private void HandleExecution(TInput value, Action<TInput> execute)
    {
        try
        {
            lock (gate)
            {
                executionCount++;
                IsExecuting.Value = executionCount > 0;
            }

            execute(value);
        }
        finally
        {
            lock (gate)
            {
                executionCount--;
                if (executionCount == 0)
                {
                    IsExecuting.Value = false;
                }
            }
        }
    }

    private async ValueTask<TOutput> HandleAsyncExecution(TInput value, Func<TInput, CancellationToken, ValueTask<TOutput>> executeAsync, CancellationToken cancellationToken)
    {
        try
        {
            lock (gate)
            {
                executionCount++;
                IsExecuting.Value = executionCount > 0;
            }

            return await executeAsync(value, cancellationToken);
        }
        finally
        {
            lock (gate)
            {
                executionCount--;
                if (executionCount == 0)
                {
                    IsExecuting.Value = false;
                }
            }
        }
    }

    protected override IDisposable SubscribeCore(Observer<TOutput> observer)
    {
        var result = completeState.TryGetResult();
        if (result != null)
        {
            observer.OnCompleted(result.Value);
            return Disposable.Empty;
        }

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

    public void Dispose()
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

            list.Dispose();
            subscription?.Dispose();
            asyncInput?.Dispose();
        }
    }

    sealed class Subscription : IDisposable
    {
        public readonly Observer<TOutput> observer;
        readonly int removeKey;
        ReactiveCommand<TInput, TOutput>? parent;

        public Subscription(ReactiveCommand<TInput, TOutput> parent, Observer<TOutput> observer)
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

public class ReactiveCommand : ReactiveCommand<Unit>
{
    public ReactiveCommand() : base()
    {
    }

    public ReactiveCommand(Action<Unit> execute) : base(execute)
    {
    }

    public ReactiveCommand(Func<Unit, CancellationToken, ValueTask> executeAsync, AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true, bool cancelOnCompleted = false, int maxSequential = -1)
        : base(executeAsync, awaitOperation, configureAwait, cancelOnCompleted, maxSequential)
    {
    }

    public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute)
        : base(canExecuteSource, initialCanExecute)
    {
    }
}

public static class ReactiveCommandExtensions
{
    public static ReactiveCommand<T> ToReactiveCommand<T>(this Observable<bool> canExecuteSource, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand<T>(canExecuteSource, initialCanExecute);
        return command;
    }

    public static ReactiveCommand<T> ToReactiveCommand<T>(this Observable<bool> canExecuteSource, Action<T> execute, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand<T>(canExecuteSource, initialCanExecute);

        var subscription = command.Subscribe(execute);
        command.CombineSubscription(subscription);

        return command;
    }

    public static ReactiveCommand<TInput, TOutput> ToReactiveCommand<TInput, TOutput>(this Observable<bool> canExecuteSource, Func<TInput, TOutput> convert, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand<TInput, TOutput>(canExecuteSource, initialCanExecute, convert);
        return command;
    }

    public static ReactiveCommand ToReactiveCommand(this Observable<bool> canExecuteSource, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand(canExecuteSource, initialCanExecute);
        return command;
    }

    public static ReactiveCommand ToReactiveCommand(this Observable<bool> canExecuteSource, Action<Unit> execute, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand(canExecuteSource, initialCanExecute);

        var subscription = command.Subscribe(execute);
        command.CombineSubscription(subscription);

        return command;
    }

    public static ReactiveCommand<T> ToReactiveCommand<T>(
        this Observable<bool> canExecuteSource, Func<T, CancellationToken, ValueTask> executeAsync,
        bool initialCanExecute = true,
        AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true,
        bool cancelOnCompleted = false, int maxSequential = -1)
    {
        var command = new ReactiveCommand<T>(canExecuteSource, initialCanExecute);

        var subscription = command.SubscribeAwait(executeAsync, static async (x, func, ct) => await func(x, ct), awaitOperation, configureAwait, cancelOnCompleted, maxSequential);
        command.CombineSubscription(subscription);

        return command;
    }

    public static ReactiveCommand<TInput, TOutput> ToReactiveCommand<TInput, TOutput>(
        this Observable<bool> canExecuteSource, Func<TInput, CancellationToken, ValueTask<TOutput>> convertAsync,
        bool initialCanExecute = true,
        AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true,
        bool cancelOnCompleted = false, int maxSequential = -1)
    {
        var command = new ReactiveCommand<TInput, TOutput>(canExecuteSource, initialCanExecute, convertAsync, awaitOperation, configureAwait, cancelOnCompleted, maxSequential);
        return command;
    }
}
