using Avalonia.Controls;
using Avalonia.Logging;
using R3;
using System;
using System.Diagnostics;
using Avalonia.Interactivity;
using R3.Avalonia;

namespace AvaloniaApplication1;

public partial class MainWindow : Window
{
    AvaloniaRenderingFrameProvider frameProvider;

    public MainWindow()
    {
        InitializeComponent();

        // initialize RenderingFrameProvider
        var topLevel = TopLevel.GetTopLevel(this);
        this.frameProvider = new AvaloniaRenderingFrameProvider(topLevel!);
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        // pass frameProvider
        Observable.EveryValueChanged(this, x => x.Width, frameProvider)
            .Subscribe(x => textBlock.Text = x.ToString());
    }

    protected override void OnClosed(EventArgs e)
    {
        frameProvider.Dispose();
    }
}
