using System;
using System.Threading;
using Godot;

namespace R3;

public static class GodotUINodeExtensions
{
    public static IDisposable SubscribeToLabel(this Observable<string> source, Label label)
    {
        return source.Subscribe(label, static (x, l) => l.Text = x);
    }

    public static IDisposable SubscribeToLabel<T>(this Observable<T> source, Label label)
    {
        return source.Subscribe(label, static (x, l) => l.Text = x?.ToString());
    }

    public static IDisposable SubscribeToLabel<T>(this Observable<T> source, Label label, Func<T, string> selector)
    {
        return source.Subscribe((label, selector), static (x, state) => state.label.Text = state.selector(x));
    }

    /// <summary>Observe Pressed event.</summary>
    public static Observable<Unit> OnPressedAsObservable(this BaseButton button, CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(h => button.Pressed += h, h => button.Pressed -= h, cancellationToken);
    }

    /// <summary>Observe Toggled with current `ButtonPressed` value on subscribe.</summary>
    public static Observable<bool> OnToggledAsObservable(this BaseButton button, CancellationToken cancellationToken = default)
    {
        if (!button.ToggleMode) return Observable.Empty<bool>();

        return Observable.Create<bool, (BaseButton, CancellationToken)>((button, cancellationToken), static (observer, state) =>
        {
            var (b, cancellationToken) = state;
            observer.OnNext(b.ButtonPressed);
            return Observable.FromEvent<BaseButton.ToggledEventHandler, bool>(h => new BaseButton.ToggledEventHandler(h), h => b.Toggled += h, h => b.Toggled -= h, cancellationToken).Subscribe(observer);
        });
    }

    /// <summary>Observe ValueChanged with current `Value` on subscribe.</summary>
    public static Observable<double> OnValueChangedAsObservable(this Godot.Range range, CancellationToken cancellationToken = default)
    {
        return Observable.Create<double, (Godot.Range, CancellationToken)>((range, cancellationToken), static (observer, state) =>
        {
            var (s, cancellationToken) = state;
            observer.OnNext(s.Value);
            return Observable.FromEvent<Godot.Range.ValueChangedEventHandler, double>(h => new Godot.Range.ValueChangedEventHandler(h), h => s.ValueChanged += h, h => s.ValueChanged -= h, cancellationToken).Subscribe(observer);
        });
    }

    /// <summary>Observe TextSubmitted event.</summary>
    public static Observable<string> OnTextSubmittedAsObservable(this LineEdit lineEdit, CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<LineEdit.TextSubmittedEventHandler, string>(h => new LineEdit.TextSubmittedEventHandler(h), h => lineEdit.TextSubmitted += h, h => lineEdit.TextSubmitted -= h, cancellationToken);
    }

    /// <summary>Observe TextChanged event.</summary>
    public static Observable<string> OnTextChangedAsObservable(this LineEdit lineEdit, CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<LineEdit.TextChangedEventHandler, string>(h => new LineEdit.TextChangedEventHandler(h), h => lineEdit.TextChanged += h, h => lineEdit.TextChanged -= h, cancellationToken);
    }

    /// <summary>Observe TextChanged event.</summary>
    public static Observable<Unit> OnTextChangedAsObservable(this TextEdit textEdit, CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(h => textEdit.TextChanged += h, h => textEdit.TextChanged -= h, cancellationToken);
    }

    /// <summary>Observe ItemSelected with current `Selected` on subscribe.</summary>
    public static Observable<long> OnItemSelectedAsObservable(this OptionButton optionButton, CancellationToken cancellationToken = default)
    {
        return Observable.Create<long, (OptionButton, CancellationToken)>((optionButton, cancellationToken), static (observer, state) =>
        {
            var (b, cancellationToken) = state;
            observer.OnNext(b.Selected);
            return Observable.FromEvent<OptionButton.ItemSelectedEventHandler, long>(h => new OptionButton.ItemSelectedEventHandler(h), h => b.ItemSelected += h, h => b.ItemSelected -= h, cancellationToken).Subscribe(observer);
        });
    }
}
