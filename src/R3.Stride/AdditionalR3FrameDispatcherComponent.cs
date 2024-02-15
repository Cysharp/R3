using Stride.Core;
using Stride.Core.Annotations;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Stride.Engine.Design;

namespace R3.Stride;
[DataContract(nameof(AdditionalR3FrameDispatcherComponent))]
[Display("additional R3 frame dispatcher")]
[ComponentCategory("R3")]
[DefaultEntityComponentProcessor(typeof(AdditionalStrideFrameDispatcherProcessor), ExecutionMode = ExecutionMode.Runtime)]
public class AdditionalR3FrameDispatcherComponent : SyncScript
{
    [DataMemberIgnore]
    public StrideFrameProvider? FrameProvider { get; private set; }
    public override void Start()
    {
        InitializeFrameProvider();
    }
    public override void Update()
    {
        if(FrameProvider != null)
        {
            this.FrameProvider.Delta.Value = Game.UpdateTime.Total.TotalSeconds;
            this.FrameProvider.Run(Game.UpdateTime.Total.TotalSeconds);
        }
    }
    internal void InitializeFrameProvider()
    {
        if(FrameProvider == null)
        {
            FrameProvider = new StrideFrameProvider(Game);
            FrameProvider.Delta = new StrongBox<double>();
        }
    }
}

public class AdditionalStrideFrameDispatcherProcessor: EntityProcessor<AdditionalR3FrameDispatcherComponent>
{
    protected override void ProcessEntityComponent(Entity entity, EntityComponent entityComponentArg, bool forceRemove)
    {
        base.ProcessEntityComponent(entity, entityComponentArg, forceRemove);
    }
    protected override void OnEntityComponentAdding(Entity entity, [NotNull] AdditionalR3FrameDispatcherComponent component, [NotNull] AdditionalR3FrameDispatcherComponent data)
    {
        component.InitializeFrameProvider();
        base.OnEntityComponentAdding(entity, component, data);
    }
}
