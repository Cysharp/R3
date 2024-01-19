using R3;
using R3.WinForms;

namespace WinFormsApp1;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        this.components ??= new System.ComponentModel.Container();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        Observable
            .EveryValueChanged(
                this,
                static form => form.Width)
            .Subscribe(x =>
            {
                this.Text = $"Width: {x:#,0}";
            })
            .AddTo(this.components);

        Observable
            .FromEventHandler(
                handler => this.button1.Click += handler,
                handler => this.button1.Click -= handler)
            .Delay(TimeSpan.FromSeconds(1))
            .Subscribe(_ => this.label1.Text = ObservableSystem.DefaultTimeProvider.GetLocalNow().ToString())
            .AddTo(this.components);
    }
}
