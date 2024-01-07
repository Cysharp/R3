using System;
using System.Threading;
using UnityEngine;

namespace R3
{
    public static class MonoBehaviourExtensions
    {
        internal static CancellationToken GetDestroyCancellationToken(this MonoBehaviour value)
        {
            // UNITY_2022_2_OR_NEWER has MonoBehavior.destroyCancellationToken
#if UNITY_2022_2_OR_NEWER
            return value.destroyCancellationToken;
#else
            return CancellationToken.None;;
#endif
        }

#if UNITY_2022_2_OR_NEWER

        public static CancellationTokenRegistration AddTo(this IDisposable disposable, MonoBehaviour value)
        {
            return value.destroyCancellationToken.Register(state =>
            {
                ((IDisposable)state!).Dispose();
            }, disposable, false);
        }

#endif
    }
}
