#if TOOLS
#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

// ObservableTrackerDebuggerPlugin creates the Observable Tracker tab in the debugger, and communicates with ObservableTrackerRuntimeHook via EditorDebuggerSessions.
[Tool]
public partial class ObservableTrackerDebuggerPlugin : EditorDebuggerPlugin
{
    // Shared header used in IPC by ObservableTracker classes.

    public const string MessageHeader = "ObservableTracker";

    // Implemented by ObservableTrackerRuntimeHook.
    public const string Message_NotifyReady = "NotifyReady";
    public const string Message_RequestActiveTasks = "RequestActiveTasks";
    public const string Message_SetEnableStates = "SetEnableStates";
    public const string Message_InvokeGCCollect = "InvokeGCCollect";

    // Implemented by ObservableTrackerDebuggerPlugin.
    public const string Message_ReceiveActiveTasks = "ReceiveActiveTasks";

    // A TrackerSession isolates each debugger session's states.
    // There's no way to know if a session has been disposed for good, so we will never remove anything from this dictionary.
    // This is similar to how it is handled in the Godot core (see: https://github.com/godotengine/godot/blob/master/modules/multiplayer/editor/multiplayer_editor_plugin.cpp)
    readonly Dictionary<int, TrackerSession> sessions = [];
    ObservableTrackerTab tab = null!;

    class TrackerSession(EditorDebuggerSession debuggerSession)
    {
        public readonly EditorDebuggerSession debuggerSession = debuggerSession;
        public readonly List<TrackingState> states = new();
        public event Action<IEnumerable<TrackingState>>? ReceivedActiveTasks;

        public void InvokeReceivedActiveTasks()
        {
            ReceivedActiveTasks?.Invoke(states);
        }
    }

    public void SetTab(ObservableTrackerTab tab)
    {
        this.tab = tab;
    }
    
    public override void _SetupSession(int sessionId)
    {
        var currentSession = GetSession(sessionId);
        sessions[sessionId] = new TrackerSession(currentSession);

        // NotifyOnSessionSetup gives the tab a reference to the debugger plugin, as well as the sessionId which is needed for messages.
        tab.NotifyOnSessionSetup(this, sessionId);
        currentSession.AddSessionTab(tab);

        // Set the process to false to avoid unnecessary processing before the session is ready.
        tab.SetProcess(false);
        
        currentSession.Started += () =>
        {
            if (IsInstanceValid(tab))
            {
                tab.SetProcess(true);
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

        if (!sessions.TryGetValue(sessionId, out var session))
        {
            // This should never happen, but just in case.
            GD.PrintErr(nameof(ObservableTrackerDebuggerPlugin) + " received message for unknown sessionId " + sessionId);
            return false;
        }

        switch (messageWithoutHeader)
        {
            case Message_ReceiveActiveTasks:
                // Only invoke event if updated.
                if (data[0].AsBool())
                {
                    session.states.Clear();
                    foreach (GDArray item in data[1].AsGodotArray())
                    {
                        var state = new TrackingState()
                        {
                            TrackingId = item[0].AsInt32(),
                            FormattedType = item[1].AsString(),
                            AddTime = new DateTime(item[2].AsInt64()),
                            StackTrace = item[3].AsString(),
                        };
                        
                        session.states.Add(state);
                    }
                    session.InvokeReceivedActiveTasks();
                }

                return true;
            case Message_NotifyReady:
                tab.NotifyOnReady();
                return true;
        }

        return default;
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

    public void SetEnableStates(int sessionId, bool enableTracking, bool enableStackTrace)
    {
        SendMessage(sessionId, MessageHeader + ":" + Message_SetEnableStates, new GDArray() { enableTracking, enableStackTrace });
    }

    public void UpdateTrackingStates(int sessionId, bool forceUpdate = false)
    {
        SendMessage(sessionId, MessageHeader + ":" + Message_RequestActiveTasks, new GDArray() { forceUpdate });
    }

    public void InvokeGCCollect(int sessionId)
    {
        SendMessage(sessionId, MessageHeader + ":" + Message_InvokeGCCollect, new GDArray() { });
    }

    void SendMessage(int sessionId, string message, GDArray data)
    {
        // Runtime Session and Debugger Session are not the same.
        if (!sessions.TryGetValue(sessionId, out var session))
        {
            session = sessions[sessionId] = new TrackerSession(GetSession(sessionId));
            tab.ReSetupSession(this, sessionId);
        }

        if (session.debuggerSession.IsActive())
            session.debuggerSession.SendMessage(message, data);
    }

    internal void Clear()
    {
        foreach (var session in sessions)
        {
            session.Value.debuggerSession.RemoveSessionTab(tab);
        }
        sessions.Clear();
    }
}

#endif
