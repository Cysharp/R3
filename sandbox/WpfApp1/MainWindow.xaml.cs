using R3;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace WpfApp1;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //var vm = new BasicUsagesViewModel();
        //vm.Input.Value = "hogemogehugahuga";
        //this.DataContext = new BasicUsagesViewModel();


        //Dispatcher.Yield(DispatcherPriority.Input);







        //Observable.EveryValueChanged(this, x => x.Width).Subscribe(x => textBlock.Text = x.ToString());
        // this.ObserveEveryValueChanged(x => x.Height).Subscribe(x => HeightText.Text = x.ToString());

        // var sw = Stopwatch.StartNew();

        //System.Reactive.Linq.Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)).Subscribe(_ =>
        //{
        //    textBlock.Text = "Hello World:" + sw.Elapsed;
        //});
        //Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
        ////    // .ObserveOnCurrentDispatcher()
        //    .Subscribe(_ =>
        //    {
        //        textBlock.Text = "Hello World:" + sw.Elapsed;
        //    });

        //Observable.TimerFrame(50, 100).Subscribe(_ =>
        //{
        //    textBlock.Text = "Hello World:" + ObservableSystem.DefaultFrameProvider.GetFrameCount();
        //});
    }

    protected override void OnClosed(EventArgs e)
    {
        (this.DataContext as IDisposable)?.Dispose();
    }
}

public class BasicUsagesViewModel : IDisposable
{
    public BindableReactiveProperty<string> Input { get; }
    public IReadOnlyBindableReactiveProperty<string> Output { get; }

    public BasicUsagesViewModel()
    {
        Input = new BindableReactiveProperty<string>("");
        Output = Input.Select(x => x.ToUpper()).ToReadOnlyBindableReactiveProperty("");
    }

    public void Dispose()
    {
        Disposable.Dispose(Input, Output);
    }
}

public class ValidationViewModel : IDisposable
{
    // Pattern 1. use EnableValidation<T> to enable DataAnnotation validation in field initializer
    [Range(0.0, 300.0)]
    public BindableReactiveProperty<double> Height { get; } = new BindableReactiveProperty<double>().EnableValidation<ValidationViewModel>();

    [Range(0.0, 300.0)]
    public BindableReactiveProperty<double> Weight { get; }

    IDisposable customValidation1Subscription;
    public BindableReactiveProperty<double> CustomValidation1 { get; set; }

    public BindableReactiveProperty<double> CustomValidation2 { get; set; }

    public ValidationViewModel()
    {
        // Pattern 2. use EnableValidation(Expression) to enable DataAnnotation validation
        Weight = new BindableReactiveProperty<double>().EnableValidation(() => Weight);

        // Pattern 3. EnableValidation() and call OnErrorResume to set custom error meessage
        CustomValidation1 = new BindableReactiveProperty<double>().EnableValidation();
        customValidation1Subscription = CustomValidation1.Subscribe(x =>
        {
            if (0.0 <= x && x <= 300.0) return;

            CustomValidation1.OnErrorResume(new Exception("value is not in range."));
        });

        // Pattern 4. simplified version of Pattern3, EnableValidation(Func<T, Exception?>)
        CustomValidation2 = new BindableReactiveProperty<double>().EnableValidation(x =>
        {
            if (0.0 <= x && x <= 300.0) return null; // null is no validate result
            return new Exception("value is not in range.");
        });
    }

    public void Dispose()
    {
        Disposable.Dispose(Height, Weight, CustomValidation1, customValidation1Subscription, CustomValidation2);
    }
}


public class CommandViewModel : IDisposable
{
    public BindableReactiveProperty<bool> OnCheck { get; }
    public ReactiveCommand<Unit> ShowMessageBox { get; }

    public CommandViewModel()
    {
        OnCheck = new BindableReactiveProperty<bool>();
        ShowMessageBox = OnCheck.ToReactiveCommand(_ =>
        {
            MessageBox.Show("clicked");
        });
    }

    public void Dispose()
    {
        Disposable.Dispose(OnCheck, ShowMessageBox);
    }
}
