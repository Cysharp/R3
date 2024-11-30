using System.Diagnostics;
using R3;

namespace MauiApp1;

public class BasicUsagesViewModel : IDisposable
{
    public BindableReactiveProperty<string> Input { get; }
    public BindableReactiveProperty<string> Output { get; }

    public ReactiveCommand<Unit> DoProcessingCommand { get; }

    public IReadOnlyBindableReactiveProperty<bool> IsProcessing { get; }

    public BasicUsagesViewModel()
    {
        Input = new BindableReactiveProperty<string>("");
        Output = Input.Select(x => x.ToUpper()).ToBindableReactiveProperty("");

        DoProcessingCommand = new(DoProcessingAsync, AwaitOperation.Parallel);

        IsProcessing =
            this.DoProcessingCommand
                .IsExecuting
                .ToReadOnlyBindableReactiveProperty(false);
    }

    private async ValueTask DoProcessingAsync(Unit input, CancellationToken token)
    {
        var now = DateTime.Now;

        Debug.WriteLine($"Starting processing of input: {now}");
        await Task.Delay(TimeSpan.FromSeconds(10), token);
        Debug.WriteLine($"Finished processing of input: {now}");
    }

    public void Dispose()
    {
        Disposable.Dispose(Input, Output, DoProcessingCommand, IsProcessing);
    }
}

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();

        var frameCount = 0;
        Observable.IntervalFrame(1)
            .Subscribe(x =>
            {
                Label2.Text = $"Frame: {frameCount++}";
            });

        this.Loaded += (sender, args) =>
        {
            var viewModel = new BasicUsagesViewModel();

            BindingContext = viewModel;
        };


    }

    void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
