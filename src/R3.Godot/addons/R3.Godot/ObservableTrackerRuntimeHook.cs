#if TOOLS
#nullable enable

using System;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

public partial class ObservableTrackerRuntimeHook : Node
{
    public override void _Ready()
    {
        // Enable tracking by default immediately on start.
        ObservableTracker.EnableTracking = true;
        ObservableTracker.EnableStackTrace = true;

        EngineDebugger.RegisterMessageCapture(ObservableTrackerDebuggerPlugin.MessageHeader,
            Callable.From((string message, GDArray data) =>
            {
                var command = message.Contains(':') ? message.Substring(message.IndexOf(':') + 1) : message;

                switch (command)
                {
                    case ObservableTrackerDebuggerPlugin.Message_RequestActiveTasks:
                        if (ObservableTracker.CheckAndResetDirty() || (data.Count > 0 && data[0].AsBool()))
                        {
                            GDArray states = new();
                            ObservableTracker.ForEachActiveTask(state =>
                            {
                                states.Add(new GDArray
                                {
                                    state.TrackingId, state.FormattedType, state.AddTime.Ticks, state.StackTrace
                                });
                            });

                            EngineDebugger.SendMessage(
                                ObservableTrackerDebuggerPlugin.MessageHeader + ":" +
                                ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks,
                                new GDArray { true, states });
                        }
                        else
                        {
                            EngineDebugger.SendMessage(
                                ObservableTrackerDebuggerPlugin.MessageHeader + ":" +
                                ObservableTrackerDebuggerPlugin.Message_ReceiveActiveTasks, new GDArray { false });
                        }

                        break;

                    case ObservableTrackerDebuggerPlugin.Message_SetEnableStates:
                        if (data.Count >= 2)
                        {
                            ObservableTracker.EnableTracking = data[0].AsBool();
                            ObservableTracker.EnableStackTrace = data[1].AsBool();
                        }

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
#endif