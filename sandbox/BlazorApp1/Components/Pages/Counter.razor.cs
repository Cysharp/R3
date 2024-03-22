using Microsoft.AspNetCore.Components;
using R3;

namespace BlazorApp1.Components.Pages;

public partial class Counter : IDisposable
{
    int currentCount = 0;
    IDisposable? subscription;

    protected override void OnInitialized()
    {
        subscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                currentCount++;
                StateHasChanged();
            });

        Observable
            .EveryValueChanged(this, x => x.currentCount)
            .Subscribe(cc => { Console.WriteLine($"Current Count: {cc}"); });
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }

    private void IncrementCount()
    {
        currentCount++;
    }
}
