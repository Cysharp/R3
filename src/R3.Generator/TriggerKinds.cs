using System.Buffers.Binary;

namespace R3.Generator;

[Flags]
public enum TriggerKinds
{
    Start,
    StartAsTask,
    Awake,
    AakeAsTask,
    OnDestroy,
    OnDestroyAsTask,
    OnDestroyAsCancellationToken,
    OnMouseDown,
    Update,


    // IEventSystemHandler
}

public class TriggerKindDescription
{
    public required string Name { get; set; }
    public bool IsTask { get; set; } = false;
    public string TypeParameter { get; set; } = "Unit";
    public string MethodArguments { get; set; } = "";
    public string OnNextArguments { get; set; } = "default";

}

public static class TriggerKindExtensions
{
    public static List<TriggerKindDescription> GetDescription(this TriggerKinds kinds)
    {
        var list = new List<TriggerKindDescription>();

        if (HasFlag(kinds, TriggerKinds.Update))
        {
            list.Add(new() { Name = "Update" });
        }

        return list;
    }

    static bool HasFlag(TriggerKinds left, TriggerKinds right)
    {
        return (left & right) == right;
    }
}
