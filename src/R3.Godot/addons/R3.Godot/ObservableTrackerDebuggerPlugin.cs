#if TOOLS
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

[Tool]
public partial class ObservableTrackerDebuggerPlugin : EditorDebuggerPlugin
{
    public const string MessageHeader = "ObservableTracker";
    public const string Message_RequestActiveTasks = "RequestActiveTasks";
    public const string Message_SetEnableStates = "SetEnableStates";
    public const string Message_InvokeGCCollect = "InvokeGCCollect";
    public const string Message_ReceiveActiveTasks = "ReceiveActiveTasks";

    public const string AllTabsGroup = "R3_Tracker_Tabs_All";

    // Static field to hold the ONLY valid instance (Highlander Principle)
    // Used to silence ghost instances after Hot Reload.
    private static ObservableTrackerDebuggerPlugin? _currentActiveInstance;

    private readonly Dictionary<int, ObservableTrackerSession> sessions = new();

    // Empty constructor to avoid side effects during Godot's internal instantiation

    public bool IsInitialized { get; private set; }

    // Explicit initialization called by GodotR3Plugin
    public void Initialize()
    {
        if (IsInitialized)
            return;
        IsInitialized = true;

        // Claim authority
        _currentActiveInstance = this;
    }

    public void Shutdown()
    {
        IsInitialized = false;
        sessions.Clear();

        if (_currentActiveInstance == this)
            _currentActiveInstance = null;
    }

    // Checks if this instance is the current valid one
    private bool IsActiveInstance()
    {
        return IsInitialized && _currentActiveInstance == this;
    }

    private void CleanUpStaleSessions()
    {
        const int MaxSessions = 5;

        if (sessions.Count > MaxSessions)
        {
            var sessionsToRemove = sessions.Keys.OrderBy(id => id).Take(sessions.Count - MaxSessions).ToList();
            foreach (var id in sessionsToRemove)
                sessions.Remove(id);
        }
    }

    public override void _SetupSession(int sessionId)
    {
        if (!IsActiveInstance())
            return;

        CleanUpStaleSessions();

        var currentSession = GetSession(sessionId);
        if (currentSession == null)
            return;

        var trackerSession = new ObservableTrackerSession();
        sessions[sessionId] = trackerSession;

        // UI Handling
        ObservableTrackerTab tab;
        var isReusingTab = false;

        var tree = EditorInterface.Singleton.GetBaseControl().GetTree();
        var sessionGroup = $"R3_Tracker_UI_Session_{sessionId}";
        var existingNodes = tree.GetNodesInGroup(sessionGroup);

        if (existingNodes.Count > 0 && existingNodes[0] is ObservableTrackerTab oldTab)
        {
            tab = oldTab;
            isReusingTab = true;
        }
        else
        {
            tab = new ObservableTrackerTab();
            tab.AddToGroup(sessionGroup);
            tab.AddToGroup(AllTabsGroup);
        }

        tab.NotifyOnSessionSetup(this, sessionId);

        if (!isReusingTab)
            currentSession.AddSessionTab(tab);

        trackerSession.Initialize(currentSession, tab);

        if (!currentSession.IsConnected("started",
                new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStarted)))
            currentSession.Connect("started",
                new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStarted));

        if (!currentSession.IsConnected("stopped",
                new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStopped)))
            currentSession.Connect("stopped",
                new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStopped));

        if (currentSession.IsActive())
            trackerSession.OnSessionStarted();
    }

    public override bool _HasCapture(string capture)
    {
        if (!IsActiveInstance())
            return false;
        return capture == MessageHeader;
    }

    public override bool _Capture(string message, GDArray data, int sessionId)
    {
        // Block ghost instances to prevent duplicate logs
        if (!IsActiveInstance())
            return false;

        if (!message.StartsWith(MessageHeader + ":"))
            return base._Capture(message, data, sessionId);

        var messageWithoutHeader = message.Substring(message.IndexOf(':') + 1);

        if (!sessions.ContainsKey(sessionId))
            _SetupSession(sessionId);

        if (sessions.TryGetValue(sessionId, out var session))
            try
            {
                switch (messageWithoutHeader)
                {
                    case Message_ReceiveActiveTasks:
                        if (data.Count > 0 && data[0].VariantType == Variant.Type.Bool && data[0].AsBool())
                            if (data.Count > 1)
                            {
                                var tasks = data[1].AsGodotArray();
                                session.States.Clear();
                                foreach (GDArray item in tasks)
                                    session.States.Add(new TrackingState
                                    {
                                        TrackingId = item[0].AsInt32(),
                                        FormattedType = item[1].AsString(),
                                        // Use DateTimeKind.Local as R3 sends local ticks
                                        AddTime = new DateTime(item[2].AsInt64(), DateTimeKind.Local),
                                        StackTrace = item[3].AsString()
                                    });
                                session.InvokeReceivedActiveTasks();
                            }

                        break;
                }
            }
            catch (Exception e)
            {
                GD.PushWarning($"[R3 Plugin] Parse error: {e.Message}");
            }

        return true;
    }

    public void ResurrectExistingTabs(SceneTree tree)
    {
        if (!IsActiveInstance())
            return;

        var existingTabs = tree.GetNodesInGroup(AllTabsGroup);
        foreach (var node in existingTabs)
            if (node is ObservableTrackerTab tab)
            {
                var sessionId = tab.SessionId;
                if (sessionId == -1)
                    continue;

                var currentSession = GetSession(sessionId);
                if (currentSession == null)
                    continue;

                var trackerSession = new ObservableTrackerSession();
                sessions[sessionId] = trackerSession;

                tab.NotifyOnSessionSetup(this, sessionId);
                trackerSession.Initialize(currentSession, tab);

                if (!currentSession.IsConnected("started",
                        new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStarted)))
                    currentSession.Connect("started",
                        new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStarted));
                if (!currentSession.IsConnected("stopped",
                        new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStopped)))
                    currentSession.Connect("stopped",
                        new Callable(trackerSession, ObservableTrackerSession.MethodName.OnSessionStopped));

                if (currentSession.IsActive())
                    trackerSession.OnSessionStarted();
            }
    }

    public void RegisterReceivedActiveTasks(int sessionId, Action<IEnumerable<TrackingState>> action)
    {
        if (IsActiveInstance())
            sessions.GetValueOrDefault(sessionId)?.RegisterReceivedActiveTasks(action);
    }

    public void UnregisterReceivedActiveTasks(int sessionId, Action<IEnumerable<TrackingState>> action)
    {
        if (IsActiveInstance())
            sessions.GetValueOrDefault(sessionId)?.UnregisterReceivedActiveTasks(action);
    }

    public void UpdateTrackingStates(int sessionId, bool forceUpdate = false)
    {
        if (IsActiveInstance())
            sessions.GetValueOrDefault(sessionId)
                ?.RequestUpdate(MessageHeader, Message_RequestActiveTasks, forceUpdate);
    }

    public void SetEnableStates(int sessionId, bool enableTracking, bool enableStackTrace)
    {
        if (IsActiveInstance())
            sessions.GetValueOrDefault(sessionId)
                ?.SetStates(MessageHeader, Message_SetEnableStates, enableTracking, enableStackTrace);
    }

    public void InvokeGCCollect(int sessionId)
    {
        if (IsActiveInstance())
            sessions.GetValueOrDefault(sessionId)?.TriggerGC(MessageHeader, Message_InvokeGCCollect);
    }
}
#endif