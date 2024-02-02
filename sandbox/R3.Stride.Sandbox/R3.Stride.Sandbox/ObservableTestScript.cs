using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;

namespace R3.Stride.Sandbox
{
    public class ObservableTestScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public override void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    Log.Info($"interval: {Game.UpdateTime.Total}");
                });
            Observable.EveryUpdate()
                .ThrottleLastFrame(60)
                .Subscribe(x =>
                {
                    Log.Info($"everyupdate - sampleframe(10): {Game.UpdateTime.Elapsed}");
                });
            Observable.EveryUpdate()
                .ThrottleLastFrame(60)
                .Subscribe(x =>
                {
                    //throw new Exception("test exception");
                });
        }

        public override void Update()
        {
        }
    }
}
