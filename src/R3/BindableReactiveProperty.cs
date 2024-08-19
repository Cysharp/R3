using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace R3;

// for binding(TriggerAction, Behavior) usage

public interface IReadOnlyBindableReactiveProperty : INotifyPropertyChanged, INotifyDataErrorInfo, IDisposable
{
    object? Value { get; }
}

public interface IReadOnlyBindableReactiveProperty<T> : IReadOnlyBindableReactiveProperty
{
    new T Value { get; }
    IReadOnlyBindableReactiveProperty<T> EnableValidation();
    IReadOnlyBindableReactiveProperty<T> EnableValidation(Func<T, Exception?> validator);
    IReadOnlyBindableReactiveProperty<T> EnableValidation<TClass>([CallerMemberName] string? propertyName = null!);
    IReadOnlyBindableReactiveProperty<T> EnableValidation(Expression<Func<IReadOnlyBindableReactiveProperty<T>?>> selfSelector);
    Observable<T> AsObservable();
}

public interface IBindableReactiveProperty : IReadOnlyBindableReactiveProperty
{
    new object? Value { get; set; }
    void OnNext(object? value);
}

public interface IBindableReactiveProperty<T> : IBindableReactiveProperty, IReadOnlyBindableReactiveProperty<T>
{
    new T Value { get; set; }
    void OnNext(T value);
    new IBindableReactiveProperty<T> EnableValidation();
    new IBindableReactiveProperty<T> EnableValidation(Func<T, Exception?> validator);
    new IBindableReactiveProperty<T> EnableValidation<TClass>([CallerMemberName] string? propertyName = null!);
    IBindableReactiveProperty<T> EnableValidation(Expression<Func<IBindableReactiveProperty<T>?>> selfSelector);
}

// all operators need to call from UI Thread(not thread-safe)

#if NET6_0_OR_GREATER
[System.Text.Json.Serialization.JsonConverter(typeof(BindableReactivePropertyJsonConverterFactory))]
#endif
public class BindableReactiveProperty<T> : ReactiveProperty<T>, IBindableReactiveProperty<T>
{
    IDisposable? subscription;

    // ctor

    public BindableReactiveProperty()
        : base()
    {
    }

    public BindableReactiveProperty(T value)
        : base(value)
    {
    }

    public BindableReactiveProperty(T value, IEqualityComparer<T>? equalityComparer)
        : base(value, equalityComparer)
    {
    }

    // WinForms reflection data binding require to impl Value in this type
    public new T Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    // ToBindableReactiveProperty

    internal BindableReactiveProperty(Observable<T> source, T initialValue, IEqualityComparer<T>? equalityComparer)
        : base(initialValue, equalityComparer)
    {
        this.subscription = source.Subscribe(new Observer(this));
    }

    protected override void DisposeCore()
    {
        subscription?.Dispose();
    }

    class Observer(BindableReactiveProperty<T> parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            parent.Value = value;
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            parent.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            parent.OnCompleted(result);
        }
    }

    // for INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnValueChanged(T value)
    {
        if (enableNotifyError)
        {
            // comes new value, require to clear error.
            var previouslyHasErrors = (errors != null && errors.Count != 0);
            errors?.Clear();

            if (validationContext != null)
            {
                if (errors == null)
                {
                    errors = new List<ValidationResult>(validationContext.ValidatorCount);
                }

                if (!validationContext.TryValidateValue(value, errors))
                {
                    ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
                }
                else if (previouslyHasErrors)
                {
                    ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
                }
            }
            else if (validator != null)
            {
                var error = validator.Invoke(value);
                if (error != null)
                {
                    OnReceiveError(error);
                }
                else if (previouslyHasErrors)
                {
                    ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
                }
            }
            else
            {
                if (previouslyHasErrors)
                {
                    // notify error was cleared.
                    ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
                }
            }
        }

        PropertyChanged?.Invoke(this, ValueChangedEventArgs.PropertyChanged);
    }

    // for INotifyDataErrorInfo

    PropertyValidationContext? validationContext;
    Func<T, Exception?>? validator;
    bool enableNotifyError = false; // default is false
    List<ValidationResult>? errors;

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
        if (!enableNotifyError) return;

        var aggregateException = exception as AggregateException;

        if (errors == null)
        {
            if (aggregateException != null)
            {
                errors = new List<ValidationResult>(aggregateException.InnerExceptions.Count);
            }
            else
            {
                errors = new List<ValidationResult>(1);
            }
        }
        errors.Clear();

        if (aggregateException != null)
        {
            foreach (var item in aggregateException.InnerExceptions)
            {
                errors.Add(new ValidationResult(item.Message));
            }
        }
        else
        {
            errors.Add(new ValidationResult(exception.Message));
        }

        ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);
    }

    public BindableReactiveProperty<T> EnableValidation()
    {
        enableNotifyError = true;
        return this;
    }

    public BindableReactiveProperty<T> EnableValidation(Func<T, Exception?> validator)
    {
        this.validator = validator;

        enableNotifyError = true;
        return this;
    }

    public BindableReactiveProperty<T> EnableValidation<TClass>([CallerMemberName] string? propertyName = null!)
    {
        var propertyInfo = typeof(TClass).GetProperty(propertyName!, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        SetValidationContext(propertyInfo!);

        enableNotifyError = true;
        return this;
    }

    public BindableReactiveProperty<T> EnableValidation(Expression<Func<BindableReactiveProperty<T>?>> selfSelector)
    {
        var memberExpression = (MemberExpression)selfSelector.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        SetValidationContext(propertyInfo);

        enableNotifyError = true;
        return this;
    }

    void SetValidationContext(PropertyInfo propertyInfo)
    {
        var display = propertyInfo.GetCustomAttribute<DisplayAttribute>();
        var attrs = AsArray(propertyInfo.GetCustomAttributes<ValidationAttribute>());

        if (attrs.Length != 0)
        {
            var context = new ValidationContext(this)
            {
                DisplayName = display?.GetName() ?? propertyInfo.Name,
                MemberName = nameof(Value),
            };

            this.validationContext = new PropertyValidationContext(context, attrs);
        }
    }

    ValidationAttribute[] AsArray(IEnumerable<ValidationAttribute> validationAttributes)
    {
        if (validationAttributes is ValidationAttribute[] array)
        {
            return array;
        }
        return validationAttributes.ToArray();
    }

    // IBindableReactiveProperty

    object? IBindableReactiveProperty.Value
    {
        get => Value;
        set => Value = (T)value!;
    }

    void IBindableReactiveProperty.OnNext(object? value)
    {
        OnNext((T)value!);
    }

    IBindableReactiveProperty<T> IBindableReactiveProperty<T>.EnableValidation() => EnableValidation();

    IBindableReactiveProperty<T> IBindableReactiveProperty<T>.EnableValidation(Func<T, Exception?> validator) => EnableValidation(validator);

    IBindableReactiveProperty<T> IBindableReactiveProperty<T>.EnableValidation<TClass>(string? propertyName) => EnableValidation<TClass>(propertyName);

    IBindableReactiveProperty<T> IBindableReactiveProperty<T>.EnableValidation(Expression<Func<IBindableReactiveProperty<T>?>> selfSelector)
    {
        var memberExpression = (MemberExpression)selfSelector.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        SetValidationContext(propertyInfo);

        enableNotifyError = true;
        return this;
    }

    // IReadOnlyBindableReactiveProperty

    object? IReadOnlyBindableReactiveProperty.Value
    {
        get => Value;
    }

    IReadOnlyBindableReactiveProperty<T> IReadOnlyBindableReactiveProperty<T>.EnableValidation() => EnableValidation();

    IReadOnlyBindableReactiveProperty<T> IReadOnlyBindableReactiveProperty<T>.EnableValidation(Func<T, Exception?> validator) => EnableValidation(validator);

    IReadOnlyBindableReactiveProperty<T> IReadOnlyBindableReactiveProperty<T>.EnableValidation<TClass>(string? propertyName) => EnableValidation<TClass>(propertyName);

    IReadOnlyBindableReactiveProperty<T> IReadOnlyBindableReactiveProperty<T>.EnableValidation(Expression<Func<IReadOnlyBindableReactiveProperty<T>?>> selfSelector)
    {
        var memberExpression = (MemberExpression)selfSelector.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;
        SetValidationContext(propertyInfo);

        enableNotifyError = true;
        return this;
    }

    public Observable<T> AsObservable()
    {
        return this;
    }
}

internal sealed class PropertyValidationContext(ValidationContext context, ValidationAttribute[] attributes)
{
    public int ValidatorCount => attributes.Length;

    public bool TryValidateValue(object? value, ICollection<ValidationResult> validationResults)
    {
        return Validator.TryValidateValue(value!, context, validationResults, attributes);
    }
}


internal static class ValueChangedEventArgs
{
    internal static readonly PropertyChangedEventArgs PropertyChanged = new PropertyChangedEventArgs("Value");
    internal static readonly DataErrorsChangedEventArgs DataErrorsChanged = new DataErrorsChangedEventArgs("Value");
}
