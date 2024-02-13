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
            var component = value.gameObject.GetComponent<R3.Triggers.ObservableDestroyTrigger>();
            if (component == null)
            {
                component = value.gameObject.AddComponent<R3.Triggers.ObservableDestroyTrigger>();
            }
            return component.GetCancellationToken();
#endif
        }

        public static CancellationTokenRegistration AddTo(this IDisposable disposable, MonoBehaviour value)
        {
            return disposable.RegisterTo(value.GetDestroyCancellationToken());
        }
    }
}
