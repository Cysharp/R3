using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace R3;

// all operators need to call from UI Thread(not thread-safe)
public class BindableReactiveProperty<T> : ReactiveProperty<T>, INotifyPropertyChanged, INotifyDataErrorInfo
{
    // for INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnSetValue(T value)
    {
        if (enableNotifyError && validationContext != null)
        {
            if (errors == null)
            {
                errors = new List<ValidationResult>(validationContext.ValidatorCount);
            }
            errors.Clear();

            if (!validationContext.TryValidateValue(value, errors))
            {
                ErrorsChanged?.Invoke(this, ValueChangedEventArgs.DataErrorsChanged);

                // set is completed(validation does not call before set) so continue call PropertyChanged
            }
        }

        PropertyChanged?.Invoke(this, ValueChangedEventArgs.PropertyChanged);
    }

    // for INotifyDataErrorInfo

    PropertyValidationContext? validationContext;
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
