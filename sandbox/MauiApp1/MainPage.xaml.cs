using R3;

namespace MauiApp1;

public class BasicUsagesViewModel : IDisposable
{
    public BindableReactiveProperty<string> Input { get; }
    public BindableReactiveProperty<string> Output { get; }

    public BasicUsagesViewModel()
    {
        Input = new BindableReactiveProperty<string>("");
        Output = Input.Select(x => x.ToUpper()).ToBindableReactiveProperty("");
    }

    public void Dispose()
    {
        Disposable.Dispose(Input, Output);
    }
}

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
