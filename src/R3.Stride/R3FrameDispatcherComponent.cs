using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Engine;
using Stride.Games;
using System.Runtime.CompilerServices;
using Stride.Core.Serialization;
using Stride.Core.Annotations;
using Stride.Engine.Design;

namespace R3.Stride
{
    [ComponentCategory("R3")]
    [Display("R3 Frame Dispatcher")]
    [DataContract(nameof(R3FrameDispatcherComponent))]
    [DefaultEntityComponentProcessor(typeof(R3FrameDispatcherProcessor), ExecutionMode = ExecutionMode.Runtime)]
    public class R3FrameDispatcherComponent : SyncScript
    {
        public override void Start()
        {
            InitializeFrameProvider();
        }
        public override void Update()
        {
            if(StrideInitializer.DefaultFrameProvider != null)
            {
                StrideInitializer.DefaultFrameProvider.Delta.Value = Game.UpdateTime.Elapsed.TotalSeconds;
                StrideInitializer.DefaultFrameProvider.Run(Game.UpdateTime.Elapsed.TotalSeconds);
            }
        }
        internal void InitializeFrameProvider()
        {
            StrideInitializer.SetDefaultObservableSystem(Game);
        }
        internal void UninitializeFrameProvider()
        {
            StrideInitializer.ClearDefaultObservableSystem();
        }
    }
    public class R3FrameDispatcherProcessor: EntityProcessor<R3FrameDispatcherComponent>
    {
        protected override void OnEntityComponentAdding(Entity entity, [NotNull] R3FrameDispatcherComponent component, [NotNull] R3FrameDispatcherComponent data)
        {
            component.InitializeFrameProvider();
            base.OnEntityComponentAdding(entity, component, data);
        }
        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] R3FrameDispatcherComponent component, [NotNull] R3FrameDispatcherComponent data)
        {
            component.UninitializeFrameProvider();
            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
