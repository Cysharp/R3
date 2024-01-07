using System.Threading;
using UnityEngine.Events;

namespace R3
{
    public static class UnityEventExtensions
    {
        public static Observable<Unit> AsObservable(this UnityEngine.Events.UnityEvent unityEvent, CancellationToken cancellationToken = default)
        {
            return Observable.FromEvent(h => new UnityAction(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        }

        public static Observable<T> AsObservable<T>(this UnityEngine.Events.UnityEvent<T> unityEvent, CancellationToken cancellationToken = default)
        {
            return Observable.FromEvent<UnityAction<T>, T>(h => new UnityAction<T>(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        }

        public static Observable<(T0 Arg0, T1 Arg1)> AsObservable<T0, T1>(this UnityEngine.Events.UnityEvent<T0, T1> unityEvent, CancellationToken cancellationToken = default)
        {
            return Observable.FromEvent<UnityAction<T0, T1>, (T0, T1)>(h =>
            {
                return new UnityAction<T0, T1>((t0, t1) =>
                {
                    h((t0, t1));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        }

        public static Observable<(T0 Arg0, T1 Arg1, T2 Arg2)> AsObservable<T0, T1, T2>(this UnityEngine.Events.UnityEvent<T0, T1, T2> unityEvent, CancellationToken cancellationToken = default)
        {
            return Observable.FromEvent<UnityAction<T0, T1, T2>, (T0, T1, T2)>(h =>
            {
                return new UnityAction<T0, T1, T2>((t0, t1, t2) =>
                {
                    h((t0, t1, t2));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        }

        public static Observable<(T0 Arg0, T1 Arg1, T2 Arg2, T3 Arg3)> AsObservable<T0, T1, T2, T3>(this UnityEngine.Events.UnityEvent<T0, T1, T2, T3> unityEvent, CancellationToken cancellationToken = default)
        {
            return Observable.FromEvent<UnityAction<T0, T1, T2, T3>, (T0, T1, T2, T3)>(h =>
            {
                return new UnityAction<T0, T1, T2, T3>((t0, t1, t2, t3) =>
                {
                    h((t0, t1, t2, t3));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h), cancellationToken);
        }
    }
}
