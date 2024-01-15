#nullable enable

using Godot;
using System;
using GDArray = Godot.Collections.Array;

namespace R3;

// Sends runtime SubscriptionTracker information to ObservableTrackerDebuggerPlugin.
// Needs to be an Autoload. Should not be instantiated manually.
public partial class ObservableTrackerRuntimeHook : Node
{
    public override void _Ready()
    {
        EngineDebugger.RegisterMessageCapture(ObservableTrackerDebuggerPlugin.MessageHeader, Callable.From((string message, GDArray data) =>
        { 
            //GD.Print(nameof(ObservableTrackerRuntimeHook) + " received " + message);
            switch (message)
            {
                case ObservableTrackerDebuggerPlugin.Message_RequestActiveTasks:
                    // data[0]: If true, force an update anyway.
                    if (SubscriptionTracker.CheckAndResetDirty() || data[0].AsBool())
                    {
                        GDArray states = new();
                        SubscriptionTracker.ForEachActiveTask(state =>
                        {
                            // DateTime is not a Variant type, so we serialize it using Ticks instead.
                            states.Add(new GDArray { state.TrackingId, state.FormattedType, state.AddTime.Ticks, state.StackTrace });
                        });
                        EngineDebugger.SendMessage(ObservableTrackerDebuggerPlugin.MessageHeader + ":" + ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks, new () { true, states });
                    }
                    else
                    {
                        EngineDebugger.SendMessage(ObservableTrackerDebuggerPlugin.MessageHeader + ":" + ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks, new () { false, });
                    }
                    break;
                case ObservableTrackerDebuggerPlugin.Message_SetEnableStates:
                    SubscriptionTracker.EnableTracking = data[0].AsBool();
                    SubscriptionTracker.EnableStackTrace = data[1].AsBool();
                    break;
                case ObservableTrackerDebuggerPlugin.Message_InvokeGCCollect:
                    GC.Collect(0);
                    break;
            }
            return true;
        }));
    }

    public override void _ExitTree()
    {
        EngineDebugger.UnregisterMessageCapture(ObservableTrackerDebuggerPlugin.MessageHeader);
    }
}
