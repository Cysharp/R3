using R3;
using R3.WPF;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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



        R3.WPF.WpfProviderInitializer.SetDefaultProviders();




        //var sw = Stopwatch.StartNew();
        //Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Subscribe(_ =>
        //{
        //    textBlock.Text = "Hello World:" + sw.Elapsed;
        //});

        Observable.TimerFrame(50, 100).Subscribe(_ =>
        {
            textBlock.Text = "Hello World:" + ObservableSystem.DefaultFrameProvider.GetFrameCount();
        });
    }
}
