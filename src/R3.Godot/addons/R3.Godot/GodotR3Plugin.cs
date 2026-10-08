#if TOOLS
#nullable enable

using System;
using Godot;

namespace R3;

[Tool]
public partial class GodotR3Plugin : EditorPlugin
{
    // Static reference to detect Hot Reload state

    // Cooldown to prevent spamming init on failure
    private int _initCooldown;

    // Public accessor for Tabs to find the active plugin instance
    public static ObservableTrackerDebuggerPlugin? CurrentDebuggerPlugin { get; private set; }

    public override void _EnterTree()
    {
        InitializeDebuggerPlugin();
    }

    public override void _ExitTree()
    {
        // Only remove the debugger plugin registration.
        // Do not nullify the static reference here to allow Hot Reload to take over.
        if (CurrentDebuggerPlugin != null)
            RemoveDebuggerPlugin(CurrentDebuggerPlugin);
    }

    public override void _DisablePlugin()
    {
        // Full cleanup when the user explicitly disables the plugin
        if (CurrentDebuggerPlugin != null)
        {
            CurrentDebuggerPlugin.Shutdown();
            RemoveDebuggerPlugin(CurrentDebuggerPlugin);
            CurrentDebuggerPlugin = null;
        }

        RemoveAutoloadSingleton(nameof(FrameProviderDispatcher));
        RemoveAutoloadSingleton(nameof(ObservableTrackerRuntimeHook));
    }

    public override void _Process(double delta)
    {
        if (_initCooldown > 0)
        {
            _initCooldown--;
            return;
        }

        // Heartbeat check: If static reference is null, Hot Reload occurred.
        if (CurrentDebuggerPlugin == null)
            InitializeDebuggerPlugin();
    }

    private void InitializeDebuggerPlugin()
    {
        if (CurrentDebuggerPlugin != null)
            return;

        try
        {
            // 1. Instantiate (State: Uninitialized)
            CurrentDebuggerPlugin = new ObservableTrackerDebuggerPlugin();

            // 2. Explicit Initialize (State: Active)
            CurrentDebuggerPlugin.Initialize();

            // 3. Register to Editor
            AddDebuggerPlugin(CurrentDebuggerPlugin);

            // 4. Resurrect existing UI Tabs
            var baseControl = EditorInterface.Singleton.GetBaseControl();
            if (baseControl != null)
                // CallDeferred to ensure the Editor UI tree is stable
                Callable.From(() =>
                {
                    if (CurrentDebuggerPlugin != null)
                        CurrentDebuggerPlugin.ResurrectExistingTabs(baseControl.GetTree());
                }).CallDeferred();

            // 5. Register Runtime Autoloads
            AddAutoloadSafe(nameof(FrameProviderDispatcher), "res://addons/R3.Godot/FrameProviderDispatcher.cs");
            AddAutoloadSafe(nameof(ObservableTrackerRuntimeHook),
                "res://addons/R3.Godot/ObservableTrackerRuntimeHook.cs");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[R3] Init failed: {e.Message}");
            CurrentDebuggerPlugin = null;
            _initCooldown = 120;
        }
    }

    private void AddAutoloadSafe(string name, string path)
    {
        if (!ProjectSettings.HasSetting("autoload/" + name))
            AddAutoloadSingleton(name, path);
    }
}
#endif