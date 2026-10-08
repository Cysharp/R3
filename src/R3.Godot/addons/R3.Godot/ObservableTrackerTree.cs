#if TOOLS
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;

namespace R3;

[Tool]
public partial class ObservableTrackerTree : Tree
{
    // Regex for parsing stack trace
    private static readonly Regex StackTraceRegex =
        new(@"\s+in\s+(?<path>.+):line\s+(?<line>\d+)", RegexOptions.Compiled);

    private static readonly string[] OperatorKeywords = new[]
    {
        "Where", "Select", "Take", "Skip", "Do",
        "Throttle", "Debounce", "Delay", "Sample",
        "CombineLatest", "Merge", "Zip", "Switch", "Amb",
        "Distinct", "ObserveOn", "SubscribeOn", "Synchronize",
        "Repeat", "Retry", "Catch", "Timeout", "IgnoreElements",
        "Materialize", "Dematerialize", "TimeInterval", "Timestamp",
        "Chunk", "Buffer", "Window", "Pairwise", "Scan"
    };

    private ObservableTrackerDebuggerPlugin? _debuggerPlugin;
    private bool _hideOperators;
    private IEnumerable<TrackingState>? _lastStates;

    private string _projectPath = "";
    private int _sessionId;
    private bool _simpleTrace = true;

    public override void _Ready()
    {
        // Normalize path for cross-platform compatibility
        var globalPath = ProjectSettings.GlobalizePath("res://");
        _projectPath = globalPath.Replace('\\', '/').TrimEnd('/');

        AllowReselect = false;
        Columns = 3;
        ColumnTitlesVisible = true;

        SetColumnTitle(0, "Type");
        SetColumnTitle(1, "Created At");
        SetColumnTitle(2, "StackTrace");

        SetColumnExpand(0, true);
        SetColumnExpand(1, true);
        SetColumnExpand(2, true);

        SetColumnExpandRatio(0, 4);
        SetColumnExpandRatio(1, 2);
        SetColumnExpandRatio(2, 6);

        SetColumnClipContent(0, true);
        SetColumnClipContent(1, true);
        SetColumnClipContent(2, true);

        HideRoot = true;
        SizeFlagsVertical = SizeFlags.ExpandFill;

        ItemActivated += OnItemActivated;
    }

    private void OnItemActivated()
    {
        var item = GetSelected();
        if (item == null)
            return;

        var traceText = item.GetText(2);
        if (string.IsNullOrWhiteSpace(traceText))
            return;

        var lines = traceText.Split('\n');
        foreach (var line in lines)
        {
            var match = StackTraceRegex.Match(line);
            if (match.Success)
            {
                var absPath = match.Groups["path"].Value.Trim();
                if (int.TryParse(match.Groups["line"].Value, out var lineNumber))
                {
                    JumpToScript(absPath, lineNumber);
                    return;
                }
            }
        }
    }

    private void JumpToScript(string absolutePath, int lineNumber)
    {
        var resPath = ProjectSettings.LocalizePath(absolutePath);

        if (!FileAccess.FileExists(resPath))
        {
            if (!FileAccess.FileExists(absolutePath))
            {
                GD.PrintErr($"[R3 Tracker] Cannot find script file: {resPath}");
                return;
            }

            resPath = absolutePath;
        }

        var script = GD.Load<Script>(resPath);
        if (script != null)
            EditorInterface.Singleton.EditScript(script, lineNumber);
    }

    public void NotifyOnSessionSetup(ObservableTrackerDebuggerPlugin debuggerPlugin, int sessionId)
    {
        _debuggerPlugin = debuggerPlugin;
        _sessionId = sessionId;
        debuggerPlugin.RegisterReceivedActiveTasks(sessionId, OnReceivedActiveTasks);
        ClearContent();
    }

    public override void _ExitTree()
    {
        _debuggerPlugin?.UnregisterReceivedActiveTasks(_sessionId, OnReceivedActiveTasks);
    }

    public void ClearContent()
    {
        Clear();
        _lastStates = null;
    }

    public void SetHideOperators(bool hide)
    {
        _hideOperators = hide;
        if (_lastStates != null)
            RefreshTree(_lastStates);
    }

    public void SetSimpleTrace(bool simple)
    {
        _simpleTrace = simple;
        if (_lastStates != null)
            RefreshTree(_lastStates);
    }

    private void OnReceivedActiveTasks(IEnumerable<TrackingState> states)
    {
        _lastStates = states;
        RefreshTree(states);
    }

    private void RefreshTree(IEnumerable<TrackingState> states)
    {
        Clear();
        var root = CreateItem();

        var count = 0;
        const int MaxItems = 1000;

        foreach (var state in states)
        {
            if (count >= MaxItems)
                break;

            if (_hideOperators)
            {
                var isOperator = OperatorKeywords.Any(keyword => state.FormattedType.Contains(keyword));
                var isReactiveProperty = state.FormattedType.Contains("ReactiveProperty");

                if (isOperator && !isReactiveProperty)
                    continue;
            }

            count++;
            var row = CreateItem(root);

            row.SetText(0, state.FormattedType);
            row.SetTooltipText(0, state.FormattedType);

            // Time Display
            var localTime = state.AddTime.ToLocalTime();
            row.SetText(1, localTime.ToString("HH:mm:ss.fff"));

            var elapsed = DateTime.Now - localTime;
            row.SetTooltipText(1, $"Created at: {localTime}\nElapsed: {elapsed.TotalSeconds:0.00}s ago");

            var displayTrace = state.StackTrace;
            if (_simpleTrace)
                displayTrace = FilterStackTrace(state.StackTrace);

            if (string.IsNullOrWhiteSpace(displayTrace) && !string.IsNullOrWhiteSpace(state.StackTrace))
                displayTrace = state.StackTrace.Split('\n').FirstOrDefault() ?? "";

            row.SetText(2, displayTrace);
            row.SetTooltipText(2, state.StackTrace + "\n\n(Double-click to jump to source)");
        }

        if (states.Count() > MaxItems)
        {
            var warning = CreateItem(root);
            warning.SetText(0, $"... {states.Count() - MaxItems} more items hidden ...");
            warning.SetSelectable(0, false);
            warning.SetCustomColor(0, new Color(1, 1, 1, 0.5f));
        }
    }

    private string FilterStackTrace(string rawTrace)
    {
        if (string.IsNullOrEmpty(rawTrace))
            return "";

        var sb = new StringBuilder();
        var lines = rawTrace.Split('\n');

        foreach (var line in lines)
        {
            var l = line.Trim();

            // Normalize slashes for cross-platform matching
            var normalizedLine = l.Replace('\\', '/');

            if (normalizedLine.IndexOf(_projectPath, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            // Filter Godot & R3 internal calls
            if (l.Contains(".InvokeGodotClassMethod") ||
                l.Contains(".HasGodotClassMethod") ||
                l.Contains(".RestoreGodotObjectData") ||
                l.Contains(".SaveGodotObjectData") ||
                l.Contains("System.Reactive") ||
                l.Contains("R3.Observable"))
                continue;

            sb.AppendLine(l);
        }

        return sb.ToString().Trim();
    }
}
#endif