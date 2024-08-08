using R3.Collections;
using System;
using UnityEngine;

namespace R3
{
    public class UnityFrameProvider : FrameProvider
    {
        public static readonly FrameProvider Initialization = new UnityFrameProvider(PlayerLoopTiming.Initialization);
        public static readonly FrameProvider EarlyUpdate = new UnityFrameProvider(PlayerLoopTiming.EarlyUpdate);
        public static readonly FrameProvider FixedUpdate = new UnityFrameProvider(PlayerLoopTiming.FixedUpdate);
        public static readonly FrameProvider PreUpdate = new UnityFrameProvider(PlayerLoopTiming.PreUpdate);
        public static readonly FrameProvider Update = new UnityFrameProvider(PlayerLoopTiming.Update);
        public static readonly FrameProvider PreLateUpdate = new UnityFrameProvider(PlayerLoopTiming.PreLateUpdate);
        public static readonly FrameProvider PostLateUpdate = new UnityFrameProvider(PlayerLoopTiming.PostLateUpdate);
        public static readonly FrameProvider TimeUpdate = new UnityFrameProvider(PlayerLoopTiming.TimeUpdate);
        public static readonly FrameProvider PostFixedUpdate = new UnityFrameProvider(PlayerLoopTiming.PostFixedUpdate);

        FreeListCore<IFrameRunnerWorkItem> list;
        readonly object gate = new object();

        internal PlayerLoopTiming PlayerLoopTiming { get; }

        internal UnityFrameProvider(PlayerLoopTiming playerLoopTiming)
        {
            this.PlayerLoopTiming = playerLoopTiming;
            this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        }

        public override long GetFrameCount()
        {
            return Time.frameCount;
        }

        public override void Register(IFrameRunnerWorkItem callback)
        {
            list.Add(callback, out _);
        }

        // called from PlayerLoop

        internal void Run()
        {
            long frameCount = Time.frameCount;

            var span = list.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                ref readonly var item = ref span[i];
                if (item != null)
                {
                    try
                    {
                        if (!item.MoveNext(frameCount))
                        {
                            list.Remove(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        list.Remove(i);
                        try
                        {
                            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                        }
                        catch { }
                    }
                }
            }
        }

        internal void Clear()
        {
            list.Clear(removeArray: true);
        }
    }
}
