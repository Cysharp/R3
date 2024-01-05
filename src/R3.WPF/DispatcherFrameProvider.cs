using System.Windows.Threading;

namespace R3.WPF;

public sealed class DispatcherFrameProvider : FrameProvider
{

    public DispatcherFrameProvider(Dispatcher dispatcher)
    {

        var foo = dispatcher.InvokeAsync(() =>
        {
        });

        
    }

    public override long GetFrameCount()
    {
        throw new NotImplementedException();
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        throw new NotImplementedException();
    }
}
