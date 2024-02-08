using R3;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();

        var startDate = DateTime.Now;
        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .Subscribe(x =>
            {
                throw new InvalidOperationException("oppeke");
                Label1.Text = $"Timer: {(DateTime.Now - startDate).TotalMilliseconds}";
            });

        var frameCount = 0;
        Observable.IntervalFrame(1)
            .Subscribe(x =>
            {
                Label2.Text = $"Frame: {frameCount++}";
            });

        Observable.Return(123)
            .ObserveOnThreadPool()
            .ObserveOnDispatcher()
            .Subscribe(x =>
            {
                Label3.Text = $"Return: {x}";
            });
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
