﻿
#nullable enable

using System;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

// Sends runtime ObservableTracker information to ObservableTrackerDebuggerPlugin.
// Needs to be an Autoload. Should not be instantiated manually.
public partial class ObservableTrackerRuntimeHook : Node
{
    public override void _Ready()
    {
#if TOOLS
        EngineDebugger.RegisterMessageCapture(ObservableTrackerDebuggerPlugin.MessageHeader, Callable.From((string message, GDArray data) =>
        {
            //GD.Print(nameof(ObservableTrackerRuntimeHook) + " received " + message);
            switch (message)
            {
                case ObservableTrackerDebuggerPlugin.Message_RequestActiveTasks:
                    // data[0]: If true, force an update anyway.
                    if (ObservableTracker.CheckAndResetDirty() || data[0].AsBool())
                    {
                        GDArray states = [];
        
                        // DateTime is not a Variant type, so we serialize it using Ticks instead.
                        ObservableTracker.ForEachActiveTask(state => states.Add(new GDArray { state.TrackingId, state.FormattedType, state.AddTime.Ticks, state.StackTrace }));
                        EngineDebugger.SendMessage(ObservableTrackerDebuggerPlugin.MessageHeader + ":" + ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks, [true, states]);
                    }
                    else
                    {
                        EngineDebugger.SendMessage(ObservableTrackerDebuggerPlugin.MessageHeader + ":" + ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks, [false]);
                    }
                    break;
                case ObservableTrackerDebuggerPlugin.Message_SetEnableStates:
                    GD.Print("Setting enable states: " + data);
                    ObservableTracker.EnableTracking = data[0].AsBool();
                    ObservableTracker.EnableStackTrace = data[1].AsBool();
                    break;
                case ObservableTrackerDebuggerPlugin.Message_InvokeGCCollect:
                    GD.Print("Invoking GC.Collect");
                    GC.Collect(0);
                    break;
            }
            return true;
        }));

        CallDeferred(nameof(NotifyObservableTrackerReady));
#endif
    }

    private void NotifyObservableTrackerReady()
    {
        if (!OS.IsDebugBuild())
            return;
#if TOOLS
        EngineDebugger.SendMessage(ObservableTrackerDebuggerPlugin.MessageHeader + ":" + ObservableTrackerDebuggerPlugin.Message_NotifyReady, new GDArray() { true });
#endif
    }

    public override void _ExitTree()
    {
#if TOOLS
        EngineDebugger.UnregisterMessageCapture(ObservableTrackerDebuggerPlugin.MessageHeader);
#endif
    }
}
