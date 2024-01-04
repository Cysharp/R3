#if NETSTANDARD2_0 || NETSTANDARD2_1

namespace R3 // namespace priority
{
    public interface IThreadPoolWorkItem
    {
        void Execute();
    }

    public static class ThreadPool
    {
        static readonly WaitCallback waitCallback = Execute;

        public static bool UnsafeQueueUserWorkItem(IThreadPoolWorkItem callBack, bool preferLocal)
        {
            return global::System.Threading.ThreadPool.UnsafeQueueUserWorkItem(waitCallback, callBack);
        }

        static void Execute(object? state)
        {
            var workItem = (IThreadPoolWorkItem)state!;
            workItem.Execute();
        }
    }
}

#endif
