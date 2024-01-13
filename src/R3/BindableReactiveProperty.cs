using System.Collections;
using System.ComponentModel;

namespace R3;

// all operators need to call from UI Thread(not thread-safe)
public class BindableReactiveProperty<T> : ReactiveProperty<T>, INotifyPropertyChanged, INotifyDataErrorInfo
{
    // for INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnSetValue(T value)
    {
        PropertyChanged?.Invoke(this, ValueChangedEventArgs.PropertyChanged);
    }

    // for INotifyDataErrorInfo

    public bool EnableNotifyError { get; init; } = true; // default is true, if false, don't allocate List<T>.

    List<Exception>? errors;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors
    {
        get
        {
            if (errors == null) return false;
            return errors.Count != 0;
        }
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (errors == null) return Enumerable.Empty<Exception>();
        return errors;
    }

    protected override void OnReceiveError(Exception exception)
    {
        if (!EnableNotifyError) return;

        if (errors == null)
        {
            errors = new List<Exception>();
        }
        errors.Add(exception);
        ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
    }

    public void ResetError()
    {
        if (errors != null)
        {
            errors.Clear();
        }
    }
}

internal static class ValueChangedEventArgs
{
    internal static readonly PropertyChangedEventArgs PropertyChanged = new PropertyChangedEventArgs("Value");
    internal static readonly DataErrorsChangedEventArgs DataErrorsChanged = new DataErrorsChangedEventArgs("Value");
}
