using R3;

namespace UnoSampleApp.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        
        Observable.EveryUpdate()
            .Index()
            .Subscribe(update, (index, state) =>
            {
                state.Text = $"Counter: {index}";
            });
        
        Observable
            .EveryValueChanged(this, x => x.ActualSize)
            .Subscribe(
                textBlock, 
                (size, state) =>
                {
                    state.Text = $"Size: {size.X} x {size.Y} y";
                });
    }
}
