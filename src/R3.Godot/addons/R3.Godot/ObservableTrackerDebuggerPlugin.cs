#if TOOLS
#nullable enable

using Godot;
using System;
using System.Collections.Generic;
using GDArray = Godot.Collections.Array;

namespace R3;

// ObservableTrackerDebuggerPlugin creates the Observable Tracker tab in the debugger, and communicates with ObservableTrackerRuntimeHook via EditorDebuggerSessions.
[Tool]
public partial class ObservableTrackerDebuggerPlugin : EditorDebuggerPlugin
{
    // Shared header used in IPC by ObservableTracker classes.
    public const string MessageHeader = "ObservableTracker";

    // Implemented by ObservableTrackerRuntimeHook.
    public const string Message_RequestActiveTasks = "RequestActiveTasks";
    public const string Message_SetEnableStates = "SetEnableStates";
    public const string Message_InvokeGCCollect = "InvokeGCCollect";

    // Implemented by ObservableTrackerDebuggerPlugin.
    public const string Message_ReceiveActiveTasks = "ReceiveActiveTasks";

    // A TrackerSession isolates each debugger session's states.
    // There's no way to know if a session has been disposed for good, so we will never remove anything from this dictionary.
    // This is similar to how it is handled in the Godot core (see: https://github.com/godotengine/godot/blob/master/modules/multiplayer/editor/multiplayer_editor_plugin.cpp)
    readonly Dictionary<int, TrackerSession> sessions = new();

    private class TrackerSession
    {
        public readonly EditorDebuggerSession debuggerSession;
        public readonly List<TrackingState> states = new();
        public event Action<IEnumerable<TrackingState>>? ReceivedActiveTasks;

        public TrackerSession(EditorDebuggerSession debuggerSession)
        {
            this.debuggerSession = debuggerSession;
        }

        public void InvokeReceivedActiveTasks()
        {
            ReceivedActiveTasks?.Invoke(states);
        }
    }

    public override void _SetupSession(int sessionId)
    {
        var currentSession = GetSession(sessionId);
        sessions[sessionId] = new TrackerSession(currentSession);

        // NotifyOnSessionSetup gives the tab a reference to the debugger plugin, as well as the sessionId which is needed for messages.
		var tab = new ObservableTrackerTab();
        tab.NotifyOnSessionSetup(this, sessionId);
        currentSession.AddSessionTab(tab);

        // As sessions don't seem to be ever disposed, we don't need to unregister these callbacks either.
        currentSession.Started += () =>
        {
            if (IsInstanceValid(tab))
            {
                tab.SetProcess(true);
                // Important! We need to tell the tab the session has started, so it can initialize the enabled states of the runtime ObservableTracker.
                tab.NotifyOnSessionStart();
            }
        };
        currentSession.Stopped += () =>
        {
            if (IsInstanceValid(tab))
            {
                tab.SetProcess(false);
            }
        };
    }

    public override bool _HasCapture(string capture)
    {
        return capture == MessageHeader;
    }

    public override bool _Capture(string message, GDArray data, int sessionId)
    {
        // When EditorDebuggerPlugin._Capture receives messages, the header isn't trimmed (unlike how it is in EngineDebugger),
        // so we need to trim it here.
        string messageWithoutHeader = message.Substring(message.IndexOf(':') + 1);
        //GD.Print(nameof(ObservableTrackerDebuggerPlugin) + " received " + messageWithoutHeader);
        switch(messageWithoutHeader)
        {
            case Message_ReceiveActiveTasks:
                // Only invoke event if updated.
                if (data[0].AsBool())
                {
                    var session = sessions[sessionId];
                    session.states.Clear();
                    foreach (GDArray item in data[1].AsGodotArray())
                    {
                        var state = new TrackingState()
                        {
                            TrackingId = item[0].AsInt32(),
                            FormattedType = item[1].AsString(),
                            AddTime = new DateTime(item[2].AsInt64()),
                            StackTrace = item[3].AsString(),
                        };;
                        session.states.Add(state);
                    }
                    session.InvokeReceivedActiveTasks();
                }
                return true;
        }
        return base._Capture(message, data, sessionId);
    }

    public void RegisterReceivedActiveTasks(int sessionId, Action<IEnumerable<TrackingState>> action)
    {
        if (sessions.Count > 0)
            sessions[sessionId].ReceivedActiveTasks += action;
    }

    public void UnregisterReceivedActiveTasks(int sessionId, Action<IEnumerable<TrackingState>> action)
    {
        if (sessions.Count > 0)
            sessions[sessionId].ReceivedActiveTasks -= action;
    }

    public void UpdateTrackingStates(int sessionId, bool forceUpdate = false)
    {
        if (sessions.Count > 0 && sessions[sessionId].debuggerSession.IsActive())
        {
            sessions[sessionId].debuggerSession.SendMessage(MessageHeader + ":" + Message_RequestActiveTasks, new () { forceUpdate });
        }
    }

    public void SetEnableStates(int sessionId, bool enableTracking, bool enableStackTrace)
    {
        if (sessions.Count > 0 && sessions[sessionId].debuggerSession.IsActive())
        {
            sessions[sessionId].debuggerSession.SendMessage(MessageHeader + ":" + Message_SetEnableStates, new () { enableTracking, enableStackTrace});
        }
    }

    public void InvokeGCCollect(int sessionId)
    {
        if (sessions.Count > 0 && sessions[sessionId].debuggerSession.IsActive())
        {
            sessions[sessionId].debuggerSession.SendMessage(MessageHeader + ":" + Message_InvokeGCCollect);
        }
    }
}
#endif
