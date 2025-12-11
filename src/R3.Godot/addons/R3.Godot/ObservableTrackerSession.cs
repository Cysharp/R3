#if TOOLS
#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

[Tool]
public partial class ObservableTrackerSession : RefCounted
{
    public readonly List<TrackingState> States = new();

    // Use WeakReference to prevent memory leaks
    private WeakReference<EditorDebuggerSession>? _debuggerSessionRef;
    private WeakReference<ObservableTrackerTab>? _tabRef;

    public void Initialize(EditorDebuggerSession debuggerSession, ObservableTrackerTab tab)
    {
        _debuggerSessionRef = new WeakReference<EditorDebuggerSession>(debuggerSession);
        _tabRef = new WeakReference<ObservableTrackerTab>(tab);
    }

    private EditorDebuggerSession? GetSession()
    {
        return _debuggerSessionRef != null && _debuggerSessionRef.TryGetTarget(out var s) ? s : null;
    }

    private ObservableTrackerTab? GetTab()
    {
        return _tabRef != null && _tabRef.TryGetTarget(out var t) && IsInstanceValid(t) ? t : null;
    }

    public void RequestUpdate(string header, string subMsg, bool forceUpdate)
    {
        var s = GetSession();
        if (s != null && s.IsActive())
            s.SendMessage(header + ":" + subMsg, new GDArray { forceUpdate });
    }

    public void SetStates(string header, string subMsg, bool enableTracking, bool enableStackTrace)
    {
        var s = GetSession();
        if (s != null && s.IsActive())
            s.SendMessage(header + ":" + subMsg, new GDArray { enableTracking, enableStackTrace });
    }

    public void TriggerGC(string header, string subMsg)
    {
        var s = GetSession();
        if (s != null && s.IsActive())
            s.SendMessage(header + ":" + subMsg);
    }

    public void OnSessionStarted()
    {
        GetTab()?.NotifyOnSessionStart();
    }

    public void OnSessionStopped()
    {
        GetTab()?.NotifyOnSessionStop();
    }

    public event Action<IEnumerable<TrackingState>>? ReceivedActiveTasks;

    public void RegisterReceivedActiveTasks(Action<IEnumerable<TrackingState>> action)
    {
        ReceivedActiveTasks += action;
    }

    public void UnregisterReceivedActiveTasks(Action<IEnumerable<TrackingState>> action)
    {
        ReceivedActiveTasks -= action;
    }

    public void InvokeReceivedActiveTasks()
    {
        ReceivedActiveTasks?.Invoke(States);
    }
}
#endif