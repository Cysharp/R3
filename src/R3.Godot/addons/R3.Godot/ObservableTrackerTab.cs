#if TOOLS
#nullable enable

using Godot;

namespace R3;

[Tool]
public partial class ObservableTrackerTab : VBoxContainer
{
    public const string EnableAutoReloadKey = "ObservableTracker_EnableAutoReloadKey";
    public const string EnableTrackingKey = "ObservableTracker_EnableTrackingKey";
    public const string EnableStackTraceKey = "ObservableTracker_EnableStackTraceKey";
    public const string HideOperatorsKey = "ObservableTracker_HideOperatorsKey";
    public const string SimpleTraceKey = "ObservableTracker_SimpleTraceKey";
    private const double RefreshInterval = 2.0;

    private ObservableTrackerDebuggerPlugin? _debuggerPlugin;

    private bool _enableAutoReload;
    private bool _enableStackTrace;
    private bool _enableTracking;
    private bool _hideOperators;

    private double _refreshTimer;
    private bool _simpleTrace;
    private ObservableTrackerTree? _tree;

    // Persist SessionId via Metadata
    public int SessionId
    {
        get
        {
            if (HasMeta("R3_SessionId"))
                return (int)GetMeta("R3_SessionId");
            return -1;
        }
        private set => SetMeta("R3_SessionId", value);
    }

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
            return;

        Name = "Observable Tracker";
        SetProcess(false);

        // Active Reconnection Logic
        var plugin = GodotR3Plugin.CurrentDebuggerPlugin;

        if (SessionId != -1 && plugin != null)
            // Hot Reload detected: Reconnect immediately
            NotifyOnSessionSetup(plugin, SessionId);
        else
            // New Instance
            InitializeUI(false);
    }

    public void NotifyOnSessionSetup(ObservableTrackerDebuggerPlugin debuggerPlugin, int sessionId)
    {
        _debuggerPlugin = debuggerPlugin;
        SessionId = sessionId;

        if (!IsInGroup(ObservableTrackerDebuggerPlugin.AllTabsGroup))
            AddToGroup(ObservableTrackerDebuggerPlugin.AllTabsGroup);

        var sessionGroup = $"R3_Tracker_UI_Session_{sessionId}";
        if (!IsInGroup(sessionGroup))
            AddToGroup(sessionGroup);

        // Force rebuild UI to fix event bindings after hot reload
        InitializeUI(true);

        if (_tree != null)
        {
            _tree.NotifyOnSessionSetup(debuggerPlugin, sessionId);
            _tree.SetHideOperators(_hideOperators);
            _tree.SetSimpleTrace(_simpleTrace);
        }

        NotifyOnSessionStart();
    }

    public void NotifyOnSessionStart()
    {
        SetProcess(true);
        if (_debuggerPlugin == null)
            return;

        _debuggerPlugin.SetEnableStates(SessionId, _enableTracking, _enableStackTrace);
        _debuggerPlugin.UpdateTrackingStates(SessionId, true);
    }

    public void NotifyOnSessionStop()
    {
        SetProcess(false);
        if (IsInstanceValid(_tree))
            _tree!.ClearContent();
    }

    public override void _Process(double delta)
    {
        // Use delta accumulation for consistent timing
        if (_enableAutoReload && _debuggerPlugin != null && SessionId != -1)
        {
            _refreshTimer += delta;
            if (_refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0;
                _debuggerPlugin.UpdateTrackingStates(SessionId);
            }
        }
    }

    private void InitializeUI(bool forceRebuild)
    {
        // 1. Cleanup existing nodes if forcing rebuild
        if (forceRebuild)
        {
            foreach (var child in GetChildren())
                child.QueueFree();
            _tree = null;
        }
        else
        {
            foreach (var child in GetChildren())
                if (child is ObservableTrackerTree t)
                {
                    _tree = t;
                    return;
                }
        }

        // 2. Build UI
        _tree = new ObservableTrackerTree();

        var headPanelLayout = new HBoxContainer();
        AddChild(headPanelLayout);

        var enableAutoReloadToggle = new CheckButton
            { Text = "AutoReload", TooltipText = "Reload automatically every 2s." };
        var enableTrackingToggle = new CheckButton { Text = "Tracking", TooltipText = "Enable R3 Tracking." };
        var enableStackTraceToggle = new CheckButton
            { Text = "StackTrace", TooltipText = "Capture StackTrace (High Cost)." };
        var hideOperatorsToggle = new CheckButton { Text = "Hide Ops", TooltipText = "Hide intermediate operators." };
        var simpleTraceToggle = new CheckButton
            { Text = "Simple Trace", TooltipText = "Hide Godot/R3/System internal calls." };

        var reloadButton = new Button { Text = "Refresh" };
        var GCButton = new Button { Text = "GC" };

        var settings = EditorInterface.Singleton.GetEditorSettings();

        // Bind Events & Settings
        enableAutoReloadToggle.ButtonPressed =
            _enableAutoReload = GetSettingOrDefault(settings, EnableAutoReloadKey, false).AsBool();
        enableAutoReloadToggle.Toggled += t =>
        {
            settings.SetSetting(EnableAutoReloadKey, t);
            _enableAutoReload = t;
        };

        enableTrackingToggle.ButtonPressed =
            _enableTracking = GetSettingOrDefault(settings, EnableTrackingKey, false).AsBool();
        enableTrackingToggle.Toggled += t =>
        {
            settings.SetSetting(EnableTrackingKey, t);
            _enableTracking = t;
            _debuggerPlugin?.SetEnableStates(SessionId, _enableTracking, _enableStackTrace);
        };

        enableStackTraceToggle.ButtonPressed =
            _enableStackTrace = GetSettingOrDefault(settings, EnableStackTraceKey, false).AsBool();
        enableStackTraceToggle.Toggled += t =>
        {
            settings.SetSetting(EnableStackTraceKey, t);
            _enableStackTrace = t;
            _debuggerPlugin?.SetEnableStates(SessionId, _enableTracking, _enableStackTrace);
        };

        hideOperatorsToggle.ButtonPressed =
            _hideOperators = GetSettingOrDefault(settings, HideOperatorsKey, true).AsBool();
        _tree.SetHideOperators(_hideOperators);
        hideOperatorsToggle.Toggled += t =>
        {
            settings.SetSetting(HideOperatorsKey, t);
            _hideOperators = t;
            if (IsInstanceValid(_tree))
                _tree.SetHideOperators(_hideOperators);
        };

        simpleTraceToggle.ButtonPressed = _simpleTrace = GetSettingOrDefault(settings, SimpleTraceKey, true).AsBool();
        _tree.SetSimpleTrace(_simpleTrace);
        simpleTraceToggle.Toggled += t =>
        {
            settings.SetSetting(SimpleTraceKey, t);
            _simpleTrace = t;
            if (IsInstanceValid(_tree))
                _tree.SetSimpleTrace(_simpleTrace);
        };

        reloadButton.Pressed += () => _debuggerPlugin?.UpdateTrackingStates(SessionId, true);
        GCButton.Pressed += () => _debuggerPlugin?.InvokeGCCollect(SessionId);

        headPanelLayout.AddChild(enableAutoReloadToggle);
        headPanelLayout.AddChild(enableTrackingToggle);
        headPanelLayout.AddChild(enableStackTraceToggle);
        headPanelLayout.AddChild(hideOperatorsToggle);
        headPanelLayout.AddChild(simpleTraceToggle);

        headPanelLayout.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.Expand });
        headPanelLayout.AddChild(reloadButton);
        headPanelLayout.AddChild(GCButton);

        AddChild(_tree);
    }

    public override void _ExitTree()
    {
        _debuggerPlugin = null;
    }

    private static Variant GetSettingOrDefault(EditorSettings settings, string key, Variant @default)
    {
        if (settings.HasSetting(key))
            return settings.GetSetting(key);
        return @default;
    }
}
#endif