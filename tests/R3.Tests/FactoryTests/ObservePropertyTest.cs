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
    public void NullablePropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([null]);

        var nipc = new ChangesProperty();

        propertyChanger.NullableInnerPropertyChanged = nipc;

        liveList.AssertEqual([null, nipc]);
    }

    [Fact]
    public void NestedPropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.InnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.InnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 1]);

        propertyChanger.InnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 1, 2]);
    }

    [Fact]
    public void NullableNestedPropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.NullableInnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.NullableInnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 1]);

        propertyChanger.NullableInnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 1, 2]);
    }

    [Fact]
    public void NullableNestedPropertyChangedWithNullableEndProperty()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([null]);

        var nipc = new ChangesProperty();

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc;

        liveList.AssertEqual([null, nipc]);
    }

    [Fact]
    public void DoubleNestedPropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.InnerPropertyChanged, x => x.InnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged.InnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.InnerPropertyChanged.InnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 1]);
    }

    [Fact]
    public void NullableDoubleNestedPropertyChanged()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 1]);
    }

    [Fact]
    public void NullableDoubleNestedPropertyChangedWithNullableEndProperty()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanged(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([null]);

        var nipc = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc;

        liveList.AssertEqual([null, nipc]);
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

    [Fact]
    public void NullablePropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([null]);

        var nipc1 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged = nipc1;

        liveList.AssertEqual([null, null]);

        var nipc2 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged = nipc2;

        liveList.AssertEqual([null, null, nipc1]);
    }

    [Fact]
    public void NestedPropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.InnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.InnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 0]);

        propertyChanger.InnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 0, 1]);
    }

    [Fact]
    public void NullableNestedPropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.NullableInnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.NullableInnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 0]);

        propertyChanger.NullableInnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 0, 1]);
    }

    [Fact]
    public void NullableNestedPropertyChangingWithNullableEndProperty()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([null]);

        var nipc1 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc1;

        liveList.AssertEqual([null, null]);

        var nipc2 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc2;

        liveList.AssertEqual([null, null, nipc1]);

        var nipc3 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc3;

        liveList.AssertEqual([null, null, nipc1, nipc2]);
    }

    [Fact]
    public void DoubleNestedPropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.InnerPropertyChanged, x => x.InnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.InnerPropertyChanged.InnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.InnerPropertyChanged.InnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 0]);

        propertyChanger.InnerPropertyChanged.InnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 0, 1]);
    }

    [Fact]
    public void NullableDoubleNestedPropertyChanging()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged, x => x.Value)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([0]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.Value = 1;

        liveList.AssertEqual([0, 0]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.Value = 2;

        liveList.AssertEqual([0, 0, 1]);
    }

    [Fact]
    public void NullableDoubleNestedPropertyChangingWithNullableEndProperty()
    {
        ChangesProperty propertyChanger = new();

        using var liveList = propertyChanger
            .ObservePropertyChanging(x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged, x => x.NullableInnerPropertyChanged)
            .ToLiveList();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([]);

        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged = new();

        liveList.AssertEqual([null]);

        var nipc1 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc1;

        liveList.AssertEqual([null, null]);

        var nipc2 = new ChangesProperty();
        propertyChanger.NullableInnerPropertyChanged.NullableInnerPropertyChanged.NullableInnerPropertyChanged = nipc2;

        liveList.AssertEqual([null, null, nipc1]);
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
