#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using PlayerLoopType = UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace R3
{
    public static class R3LoopRunners
    {
        public struct Initialization { };
        public struct EarlyUpdate { };
        public struct FixedUpdate { };
        public struct PreUpdate { };
        public struct Update { };
        public struct PreLateUpdate { };
        public struct PostLateUpdate { };
        public struct TimeUpdate { };
    }

    internal enum PlayerLoopTiming
    {
        Initialization = 0,
        EarlyUpdate = 1,
        FixedUpdate = 2,
        PreUpdate = 3,
        Update = 4,
        PreLateUpdate = 5,
        PostLateUpdate = 6,
        TimeUpdate = 7,
    }

    public static class PlayerLoopHelper
    {
        internal static string ApplicationDataPath => applicationDataPath; // used for editor window
        static string applicationDataPath;

        static UnityFrameProvider[] runners;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            try
            {
                applicationDataPath = Application.dataPath;
            }
            catch { }

#if UNITY_EDITOR
            // When domain reload is disabled, re-initialization is required when entering play mode; 
            // otherwise, pending tasks will leak between play mode sessions.
            var domainReloadDisabled = UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload);
            if (!domainReloadDisabled && runners != null) return;
#else
            if (runners != null) return; // already initialized
#endif

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            Initialize(ref playerLoop);
        }

        public static void Initialize(ref PlayerLoopSystem playerLoop)
        {
            runners = new UnityFrameProvider[8];

            var newLoop = playerLoop.subSystemList.ToArray();

            // Initialization
            InsertLoop(newLoop, typeof(PlayerLoopType.Initialization), typeof(R3LoopRunners.Initialization), runners[0] = (UnityFrameProvider)UnityFrameProvider.Initialization);
            InsertLoop(newLoop, typeof(PlayerLoopType.EarlyUpdate), typeof(R3LoopRunners.EarlyUpdate), runners[1] = (UnityFrameProvider)UnityFrameProvider.EarlyUpdate);
            InsertLoop(newLoop, typeof(PlayerLoopType.FixedUpdate), typeof(R3LoopRunners.FixedUpdate), runners[2] = (UnityFrameProvider)UnityFrameProvider.FixedUpdate);
            InsertLoop(newLoop, typeof(PlayerLoopType.PreUpdate), typeof(R3LoopRunners.PreUpdate), runners[3] = (UnityFrameProvider)UnityFrameProvider.PreUpdate);
            InsertLoop(newLoop, typeof(PlayerLoopType.Update), typeof(R3LoopRunners.Update), runners[4] = (UnityFrameProvider)UnityFrameProvider.Update);
            InsertLoop(newLoop, typeof(PlayerLoopType.PreLateUpdate), typeof(R3LoopRunners.PreLateUpdate), runners[5] = (UnityFrameProvider)UnityFrameProvider.PreLateUpdate);
            InsertLoop(newLoop, typeof(PlayerLoopType.PostLateUpdate), typeof(R3LoopRunners.PostLateUpdate), runners[6] = (UnityFrameProvider)UnityFrameProvider.PostLateUpdate);
            InsertLoop(newLoop, typeof(PlayerLoopType.TimeUpdate), typeof(R3LoopRunners.TimeUpdate), runners[7] = (UnityFrameProvider)UnityFrameProvider.TimeUpdate);

            playerLoop.subSystemList = newLoop;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void InsertLoop(PlayerLoopSystem[] loopSystems, Type loopType, Type loopRunnerType, UnityFrameProvider frameProvider)
        {
            var i = FindLoopSystemIndex(loopSystems, loopType);
            ref var loop = ref loopSystems[i];
            loop.subSystemList = InsertRunner(loop.subSystemList, loopRunnerType, frameProvider);
        }

        static int FindLoopSystemIndex(PlayerLoopSystem[] playerLoopList, Type systemType)
        {
            for (int i = 0; i < playerLoopList.Length; i++)
            {
                if (playerLoopList[i].type == systemType)
                {
                    return i;
                }
            }

            throw new Exception("Target PlayerLoopSystem does not found. Type:" + systemType.FullName);
        }

        static PlayerLoopSystem[] InsertRunner(PlayerLoopSystem[] subSystemList, Type loopRunnerType, UnityFrameProvider runner)
        {

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingEditMode)
                {
                    // run rest action before clear.
                    if (runner != null)
                    {
                        runner.Run();
                        runner.Clear();
                    }
                }
            };
#endif

            var source = subSystemList.Where(x => x.type != loopRunnerType).ToArray(); // remove duplicate(initialized previously)
            var dest = new PlayerLoopSystem[source.Length + 1];

            Array.Copy(source, 0, dest, 1, source.Length);

            // insert first
            dest[0] = new PlayerLoopSystem
            {
                type = loopRunnerType,
                updateDelegate = runner.Run
            };

            return dest;
        }

#if UNITY_EDITOR

        [InitializeOnLoadMethod]
        static void InitOnEditor()
        {
            // Execute the play mode init method
            Init();

            // register an Editor update delegate, used to forcing playerLoop update
            EditorApplication.update += ForceEditorPlayerLoopUpdate;
        }

        private static void ForceEditorPlayerLoopUpdate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                // Not in Edit mode, don't interfere
                return;
            }

            if (runners != null)
            {
                foreach (var item in runners)
                {
                    if (item != null) item.Run();
                }
            }
        }

#endif
    }
}
