#nullable enable

using System.Runtime.CompilerServices;

namespace R3;

public partial class FrameProviderDispatcher : global::Godot.Node
{
    StrongBox<double> processDelta = new StrongBox<double>();
    StrongBox<double> physicsProcessDelta = new StrongBox<double>();

    public override void _Ready()
    {
        GodotProviderInitializer.SetDefaultObservableSystem();

        ((GodotFrameProvider)GodotFrameProvider.Process).Delta = processDelta;
        ((GodotFrameProvider)GodotFrameProvider.PhysicsProcess).Delta = physicsProcessDelta;
    }

    public override void _Process(double delta)
    {
        processDelta.Value = delta;
        ((GodotTimeProvider)GodotTimeProvider.Process).time += delta;
        ((GodotFrameProvider)GodotFrameProvider.Process).Run(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        physicsProcessDelta.Value = delta;
        ((GodotTimeProvider)GodotTimeProvider.PhysicsProcess).time += delta;
        ((GodotFrameProvider)GodotFrameProvider.PhysicsProcess).Run(delta);
    }
}
