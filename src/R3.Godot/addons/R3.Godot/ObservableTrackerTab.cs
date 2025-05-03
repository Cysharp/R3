#if TOOLS
#nullable enable

using System;
using Godot;
using GDArray = Godot.Collections.Array;

namespace R3;

[Tool]
public partial class ObservableTrackerTab : VBoxContainer
{
    public const string EnableAutoReloadKey = "ObservableTracker_EnableAutoReloadKey";
    public const string EnableTrackingKey = "ObservableTracker_EnableTrackingKey";
    public const string EnableStackTraceKey = "ObservableTracker_EnableStackTraceKey";

    HBoxContainer headPanelLayout = null!;

    CheckButton enableAutoReloadToggle = null!;
    CheckButton enableTrackingToggle = null!;
    CheckButton enableStackTraceToggle = null!;

    Button reloadButton = null!;
    Button GCButton = null!;

    ObservableTrackerDebuggerPlugin debuggerPlugin = null!;
    ObservableTrackerTree? tree;
    
    int sessionId;
    bool enableAutoReload, enableTracking, enableStackTrace;
    bool isSessionSetup = false;
    int interval = 0;
    
    public void NotifyOnSessionSetup(ObservableTrackerDebuggerPlugin debuggerPlugin, int sessionId)
    {
        this.sessionId = sessionId;
        this.debuggerPlugin = debuggerPlugin;
        isSessionSetup = true;
        tree ??= new ObservableTrackerTree();
        tree.NotifyOnSessionSetup(debuggerPlugin!, sessionId);
    }

    public void ReSetupSession(ObservableTrackerDebuggerPlugin debuggerPlugin, int sessionId)
    {
        this.sessionId = sessionId;
        this.debuggerPlugin = debuggerPlugin;
        isSessionSetup = true;
        tree ??= new ObservableTrackerTree();
        tree.ReSetupSession(debuggerPlugin!, sessionId);
    }
    
    public void NotifyOnReady()
    {
        tree?.Clear();
        debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
    }

    public ObservableTrackerTab()
    {
        Name = "Observable Tracker";
        tree ??= new ObservableTrackerTree();
        
        // Toggle buttons (top left)
        enableAutoReloadToggle = new CheckButton
        {
            Text = "Enable AutoReload",
            TooltipText = "Reload automatically."
        };
        enableTrackingToggle = new CheckButton
        {
            Text = "Enable Tracking",
            TooltipText = "Start to track Observable subscription. Performance impact: low"
        };
        enableStackTraceToggle = new CheckButton
        {
            Text = "Enable StackTrace",
            TooltipText = "Capture StackTrace when subscribed. Performance impact: high"
        };
    }
    
    public override void _Ready()
    {
        GD.Print("ObservableTrackerTab._Ready called");
        bool wasSetup = isSessionSetup;
        ObservableTrackerDebuggerPlugin? savedPlugin = debuggerPlugin;

        // Head panel
        headPanelLayout = new HBoxContainer();
        headPanelLayout.SetAnchor(Side.Left, 0);
        headPanelLayout.SetAnchor(Side.Right, 0);
        AddChild(headPanelLayout);

        // For every button: Initialize pressed state and subscribe to Toggled event.
        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        enableAutoReloadToggle.ButtonPressed = enableAutoReload = GetSettingOrDefault(settings, EnableAutoReloadKey, false).AsBool();
        enableAutoReloadToggle.Connect(CheckButton.SignalName.Toggled, Callable.From<bool>(EnableAutoReloadToggleOnToggled));
        enableTrackingToggle.ButtonPressed = enableTracking = GetSettingOrDefault(settings, EnableTrackingKey, false).AsBool();
        enableTrackingToggle.Connect(CheckButton.SignalName.Toggled, Callable.From<bool>(EnableTrackingToggleOnToggled));
        enableStackTraceToggle.ButtonPressed = enableStackTrace = GetSettingOrDefault(settings, EnableStackTraceKey, false).AsBool();
        enableStackTraceToggle.Connect(CheckButton.SignalName.Toggled, Callable.From<bool>(EnableStackTraceToggleOnToggled));
        
        // Regular buttons (top right)
        reloadButton = new Button
        {
            Text = "Reload",
            TooltipText = "Reload View."
        };
        GCButton = new Button
        {
            Text = "GC.Collect",
            TooltipText = "Invoke GC.Collect."
        };

        reloadButton.Connect(BaseButton.SignalName.Pressed, Callable.From(OnReloadButtonPressed));
        GCButton.Connect(BaseButton.SignalName.Pressed, Callable.From(OnGcButtonPressed));

        // Button layout.

        headPanelLayout.AddChild(enableAutoReloadToggle);
        headPanelLayout.AddChild(enableTrackingToggle);
        headPanelLayout.AddChild(enableStackTraceToggle);
        // Kind of like Unity's FlexibleSpace. Pushes the first three buttons to the left, and the remaining buttons to the right.

        headPanelLayout.AddChild(new Control()
        {
            SizeFlagsHorizontal = SizeFlags.Expand,
        });
        headPanelLayout.AddChild(reloadButton);
        headPanelLayout.AddChild(GCButton);

        // Tree goes last.
        AddChild(tree);

        // 確保不會丟失設置
        if (wasSetup && savedPlugin != null)
        {
            isSessionSetup = true;
            debuggerPlugin = savedPlugin;
            GD.Print("Restored session setup after _Ready");
        }
    }

    public void EnableAutoReloadToggleOnToggled(bool toggledOn)
    {
        if (!IsSessionSetup())
        {
            GD.PrintErr("Session is not setup.");
            return;
        }

        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        settings.SetSetting(EnableAutoReloadKey, toggledOn);
        enableAutoReload = toggledOn;
    }

    public void EnableTrackingToggleOnToggled(bool toggledOn)
    {
        if (!IsSessionSetup())
        {
            GD.PrintErr("Session is not setup.");
            return;
        }

        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        settings.SetSetting(EnableTrackingKey, toggledOn);
        enableTracking = toggledOn;
        debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
    }

    void EnableStackTraceToggleOnToggled(bool toggledOn)
    {
        if (!IsSessionSetup())
        {
            GD.PrintErr("Session is not setup.");
            return;
        }

        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        settings.SetSetting(EnableStackTraceKey, toggledOn);
        enableStackTrace = toggledOn;
        debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
    }
    void OnGcButtonPressed()
    {
        if (!IsSessionSetup())
        {
            GD.PrintErr("Session is not setup.");
            return;
        }

        debuggerPlugin!.InvokeGCCollect(sessionId);
    }

    void OnReloadButtonPressed()
    {
        if (!IsSessionSetup())
        {
            GD.PrintErr("Session is not setup.");
            return;
        }

        debuggerPlugin!.UpdateTrackingStates(sessionId, true);
    }

    public override void _Process(double delta)
    {
        if (enableAutoReload)
        {
            if (interval++ % 120 == 0)
            {
                debuggerPlugin!.UpdateTrackingStates(sessionId);
            }
        }
    }

    static Variant GetSettingOrDefault(EditorSettings settings, string key, Variant @default)
    {
        if (settings.HasSetting(key))
        {
            return settings.GetSetting(key);
        }
        else
        {
            return @default;
        }
    }

    internal bool IsSessionSetup()
    {
        return isSessionSetup && debuggerPlugin != null;
    }
}

#endif
