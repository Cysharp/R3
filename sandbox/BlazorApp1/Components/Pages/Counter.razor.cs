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
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }
}
