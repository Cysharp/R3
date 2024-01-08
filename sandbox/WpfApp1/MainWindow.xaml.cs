using R3;
using System.Diagnostics;
using System.Windows;

namespace WpfApp1;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();



        //Dispatcher.Yield(DispatcherPriority.Input);







        //Observable.EveryValueChanged(this, x => x.Width).Subscribe(x => textBlock.Text = x.ToString());
        // this.ObserveEveryValueChanged(x => x.Height).Subscribe(x => HeightText.Text = x.ToString());

        var sw = Stopwatch.StartNew();

        //System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Subscribe(_ =>
        //{
        //    textBlock.Text = "Hello World:" + sw.Elapsed;
        //});
        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
        //    // .ObserveOnCurrentDispatcher()
            .Subscribe(_ =>
            {
                textBlock.Text = "Hello World:" + sw.Elapsed;
            });

        //Observable.TimerFrame(50, 100).Subscribe(_ =>
        //{
        //    textBlock.Text = "Hello World:" + ObservableSystem.DefaultFrameProvider.GetFrameCount();
        //});
    }
}
