#if TOOLS
#nullable enable

using Godot;
using System;

namespace R3;

[Tool]
public partial class ObservableTrackerTab : VBoxContainer
{
    public const string EnableAutoReloadKey = "ObservableTracker_EnableAutoReloadKey";
    public const string EnableTrackingKey = "ObservableTracker_EnableTrackingKey";
    public const string EnableStackTraceKey = "ObservableTracker_EnableStackTraceKey";
    bool enableAutoReload, enableTracking, enableStackTrace;
    ObservableTrackerTree? tree;
    ObservableTrackerDebuggerPlugin? debuggerPlugin;
    int interval = 0;
    int sessionId = 0;

    public void NotifyOnSessionSetup(ObservableTrackerDebuggerPlugin debuggerPlugin, int sessionId)
    {
        this.debuggerPlugin = debuggerPlugin;
        this.sessionId = sessionId;
        tree ??= new ObservableTrackerTree();
        tree.NotifyOnSessionSetup(debuggerPlugin!, sessionId);
    }

    public void NotifyOnSessionStart()
    {
        debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
    }

    public override void _Ready()
    {
        Name = "Observable Tracker";

        tree ??= new ObservableTrackerTree();
        
        // Head panel
        var headPanelLayout = new HBoxContainer();
        headPanelLayout.SetAnchor(Side.Left, 0);
        headPanelLayout.SetAnchor(Side.Right, 0);
        AddChild(headPanelLayout);

        // Toggle buttons (top left)
        var enableAutoReloadToggle = new CheckButton
        {
            Text = "Enable AutoReload",
            TooltipText = "Reload automatically."
        };
        var enableTrackingToggle = new CheckButton
        {
            Text = "Enable Tracking",
            TooltipText = "Start to track Observable subscription. Performance impact: low"
        };
        var enableStackTraceToggle = new CheckButton
        {
            Text = "Enable StackTrace",
            TooltipText = "Capture StackTrace when subscribed. Performance impact: high"
        };

        // For every button: Initialize pressed state and subscribe to Toggled event.
        EditorSettings settings = EditorInterface.Singleton.GetEditorSettings();
        enableAutoReloadToggle.ButtonPressed = enableAutoReload = GetSettingOrDefault(settings, EnableAutoReloadKey, false).AsBool();
        enableAutoReloadToggle.Toggled += toggledOn =>
        {
            settings.SetSetting(EnableAutoReloadKey, toggledOn);
            enableAutoReload = toggledOn;
        };
        enableTrackingToggle.ButtonPressed = enableTracking = GetSettingOrDefault(settings, EnableTrackingKey, false).AsBool();
        enableTrackingToggle.Toggled += toggledOn =>
        {
            settings.SetSetting(EnableTrackingKey, toggledOn);
            enableTracking = toggledOn;
            debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
        };
        enableStackTraceToggle.ButtonPressed = enableStackTrace = GetSettingOrDefault(settings, EnableStackTraceKey, false).AsBool();
        enableStackTraceToggle.Toggled += toggledOn =>
        {
            settings.SetSetting(EnableStackTraceKey, toggledOn);
            enableStackTrace = toggledOn;
            debuggerPlugin!.SetEnableStates(sessionId, enableTracking, enableStackTrace);
        };

        // Regular buttons (top right)
        var reloadButton = new Button
        {
            Text = "Reload",
            TooltipText = "Reload View."
        };
        var GCButton = new Button
        {
            Text = "GC.Collect",
            TooltipText = "Invoke GC.Collect."
        };

        reloadButton.Pressed += () =>
        {
            debuggerPlugin!.UpdateTrackingStates(sessionId, true);
        };
        GCButton.Pressed += () =>
        {
            debuggerPlugin!.InvokeGCCollect(sessionId);
        };

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
}
#endif
