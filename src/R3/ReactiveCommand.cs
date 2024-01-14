using System.Runtime.CompilerServices;
using System.Windows.Input; // for XAML binding

namespace R3;

public class ReactiveCommand<T> : Observable<T>, ICommand, IDisposable
{
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)
    IDisposable subscription; // from canExecuteSource (and onNext).
    bool canExecute; // set from observable sequence

    public event EventHandler? CanExecuteChanged;

    public ReactiveCommand()
    {
        this.canExecute = true;
        this.subscription = Disposable.Empty;
    }

    public ReactiveCommand(Action<T> execute)
    {
        this.canExecute = true;
        this.subscription = this.Subscribe(execute);
    }

    public ReactiveCommand(Observable<bool> canExecuteSource, bool initialCanExecute)
    {
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

    public static ReactiveCommand<Unit> ToReactiveCommand(this Observable<bool> canExecuteSource, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand<Unit>(canExecuteSource, initialCanExecute);
        return command;
    }

    public static ReactiveCommand<Unit> ToReactiveCommand(this Observable<bool> canExecuteSource, Action<Unit> execute, bool initialCanExecute = true)
    {
        var command = new ReactiveCommand<Unit>(canExecuteSource, initialCanExecute);

        var subscription = command.Subscribe(execute);
        command.CombineSubscription(subscription);

        return command;
    }
}
