namespace R3
{
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
}
