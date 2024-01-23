using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Engine;
using Stride.Games;
using System.Runtime.CompilerServices;

namespace R3.Stride
{
    [ComponentCategory("R3")]
    [Display("R3 Frame Dispatcher")]
    public class R3FrameDispatcherComponent : SyncScript
    {
        public override void Start()
        {
            StrideInitializer.SetDefaultObservableSystem(Game);
        }
        public override void Update()
        {
            if(StrideInitializer.DefaultFrameProvider != null)
            {
                StrideInitializer.DefaultFrameProvider.Delta.Value = Game.UpdateTime.Elapsed.TotalSeconds;
                StrideInitializer.DefaultFrameProvider.Run(Game.UpdateTime.Elapsed.TotalSeconds);
            }
        }
    }
}
