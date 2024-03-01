using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace R3.Tests.FactoryTests;

public class ObservePropertyTest
{
    [Fact]
    public void PropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([0]);

        propertyChanger.Value = 1;

        liveList.AssertEqual([0, 1]);
    }

    [Fact]
    public void PropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([0]);

        propertyChanger.Value = 1;

        liveList.AssertEqual([0, 0]);
    }

    class ChangesProperty : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _value;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Value
        {
            get => _value;
            set => SetField(ref _value, value);
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
