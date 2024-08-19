using R3;
using R3.WinForms;
using System.ComponentModel;
using System.Diagnostics;

namespace WinFormsApp1;

public partial class Form1 : Form
{
    private readonly BindableReactiveProperty<string> rp = new("");

    public Form1()
    {
        InitializeComponent();

        this.components ??= new System.ComponentModel.Container();


        // Bind label1.Text to rp.Value
        label1.DataBindings.Add("Text", rp, "Value");
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        button1.Click += button1_Click;

        var prop = TypeDescriptor.GetProperties(rp).Find("Value", true);
        Debug.WriteLine((
            prop!.ComponentType.Name,    // "ReactiveProperty`1" (not Bindable)
            prop.SupportsChangeEvents   // false
        ));

        //Observable
        //    .EveryValueChanged(
        //        this,
        //        static form => form.Width)
        //    .Subscribe(x =>
        //    {
        //        this.Text = $"Width: {x:#,0}";
        //    })
        //    .AddTo(this.components);

        //Observable
        //    .FromEventHandler(
        //        handler => this.button1.Click += handler,
        //        handler => this.button1.Click -= handler)
        //    .Delay(TimeSpan.FromSeconds(1))
        //    .Subscribe(_ => this.label1.Text = ObservableSystem.DefaultTimeProvider.GetLocalNow().ToString())
        //    .AddTo(this.components);
    }

    public void button1_Click(object? sender, EventArgs e)
    {
        // This change won't be notified to label1
        rp.Value += "X";
    }
}
