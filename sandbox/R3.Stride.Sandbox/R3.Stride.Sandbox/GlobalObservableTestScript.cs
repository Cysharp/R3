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
    public class GlobalObservableTestScript : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            Observable.EveryUpdate()
                .ThrottleLastFrame(120)
                .Subscribe(_ =>
                {
                    Log.Info($"global observable test: {Game.UpdateTime.FrameCount}, {Game.UpdateTime.Total}");
                });
        }

        public override void Update()
        {
        }
    }
}
