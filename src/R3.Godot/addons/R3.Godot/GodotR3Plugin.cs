#if TOOLS
#nullable enable

using Godot;

namespace R3;

[Tool]
public partial class GodotR3Plugin : EditorPlugin
{
    ObservableTrackerDebuggerPlugin? observableTrackerDebugger = null!;
    ObservableTrackerTab? tab = null!;

    public override void _EnterTree()
    {
        tab ??= new ObservableTrackerTab();
        observableTrackerDebugger ??= new ObservableTrackerDebuggerPlugin();
        observableTrackerDebugger.SetTab(tab);
        AddDebuggerPlugin(observableTrackerDebugger);
        // Automatically install autoloads here for ease of use.
        AddAutoloadSingleton(nameof(FrameProviderDispatcher), "res://addons/R3.Godot/FrameProviderDispatcher.cs");
        AddAutoloadSingleton(nameof(ObservableTrackerRuntimeHook), "res://addons/R3.Godot/ObservableTrackerRuntimeHook.cs");
    }

    public override void _ExitTree()
    {
        if (observableTrackerDebugger != null)
        {
            RemoveDebuggerPlugin(observableTrackerDebugger);
            observableTrackerDebugger.Clear();
            observableTrackerDebugger = null;
        }
        
        if (tab != null)
        {
            tab.QueueFree();
            tab = null;
        }
        
        RemoveAutoloadSingleton(nameof(FrameProviderDispatcher));
        RemoveAutoloadSingleton(nameof(ObservableTrackerRuntimeHook));
    }
}
#endif
