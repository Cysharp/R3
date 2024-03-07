using System.ComponentModel;
using System.Runtime.CompilerServices;
using R3.Tests.FactoryTests;

namespace R3.Tests;

public class BindTest
{
    [Fact]
    public void NotifyPropertyChangedTest()
    {
        ChangesProperty propertyChanger1 = new();
        propertyChanger1.Value = 1;

        ChangesProperty propertyChanger2 = new();
        propertyChanger2.Value = 2;

        propertyChanger1
            .TwoWayBind(
                x => x.Value, (value, x) => value.Value = x,
                propertyChanger2, x => x.Value, (value, x) => value.Value = x);

        Assert.Equal(propertyChanger1.Value, 1);
        Assert.Equal(propertyChanger2.Value, 1);

        propertyChanger1.Value = 2;

        Assert.Equal(propertyChanger1.Value, 2);
        Assert.Equal(propertyChanger2.Value, 2);

        propertyChanger2.Value = 3;

        Assert.Equal(propertyChanger1.Value, 3);
        Assert.Equal(propertyChanger2.Value, 3);
    }

    class ChangesProperty : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _value;
        private ChangesProperty _innerPropertyChanged = default!;
        private ChangesProperty? _nullableInnerPropertyChanged;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        public ChangesProperty InnerPropertyChanged
        {
            get => _innerPropertyChanged;
            set => SetField(ref _innerPropertyChanged, value);
        }

        public ChangesProperty? NullableInnerPropertyChanged
        {
            get => _nullableInnerPropertyChanged;
            set => SetField(ref _nullableInnerPropertyChanged, value);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
