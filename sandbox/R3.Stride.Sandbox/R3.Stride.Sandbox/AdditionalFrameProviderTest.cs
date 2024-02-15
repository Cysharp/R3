using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine;

namespace R3.Stride.Sandbox;
public class AdditionalFrameProviderTest: StartupScript
{
    IDisposable _Subscription;
    public override void Start()
    {
        var component = Entity.Get<AdditionalR3FrameDispatcherComponent>();
        _Subscription = Observable.IntervalFrame(24, component.FrameProvider)
            .Subscribe(_ =>
            {
                Log.Info($"additional frameprovider: {Game.UpdateTime.Total}");
            });
    }
    public override void Cancel()
    {
        _Subscription?.Dispose();
        _Subscription = null;
    }
}
