using R3.Triggers;
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

        /// <summary>Dispose self on target gameObject has been destroyed. Return value is self disposable.</summary>
        public static T AddTo<T>(this T disposable, GameObject gameObject)
            where T : IDisposable
        {
            if (gameObject == null)
            {
                disposable.Dispose();
                return disposable;
            }

            var trigger = gameObject.GetComponent<ObservableDestroyTrigger>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<ObservableDestroyTrigger>();
            }

            // If gameObject is deactive, does not raise OnDestroy, watch and invoke trigger.
            if (!trigger.IsActivated && !trigger.gameObject.activeInHierarchy)
            {
                trigger.TryStartActivateMonitoring();
            }

            trigger.AddDisposableOnDestroy(disposable);
            return disposable;
        }

        /// <summary>Dispose self on target gameObject has been destroyed. Return value is self disposable.</summary>
        public static T AddTo<T>(this T disposable, Component gameObjectComponent)
            where T : IDisposable
        {
            if (gameObjectComponent == null)
            {
                disposable.Dispose();
                return disposable;
            }

#if UNITY_2022_2_OR_NEWER
            if (gameObjectComponent.gameObject.activeInHierarchy && gameObjectComponent is MonoBehaviour mb)
            {
                // gameObject is Awaked, no need to use ObservableDestroyTrigger
                disposable.RegisterTo(mb.destroyCancellationToken);
                return disposable;
            }
#endif

            // Add ObservableDestroyTrigger
            return AddTo(disposable, gameObjectComponent.gameObject);
        }
    }
}
