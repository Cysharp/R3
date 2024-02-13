using System.Threading;
using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableDestroyTrigger : MonoBehaviour
    {
        bool calledDestroy = false;
        Subject<Unit> onDestroy;
        CancellationTokenSource cancellationTokenSource;

        public CancellationToken GetCancellationToken()
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
                if (calledDestroy)
                {
                    cancellationTokenSource.Cancel();
                }
            }

            return cancellationTokenSource.Token;
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        void OnDestroy()
        {
            if (!calledDestroy)
            {
                calledDestroy = true;
                if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
                if (onDestroy != null) { onDestroy.OnNext(Unit.Default); onDestroy.OnCompleted(); }
            }
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public Observable<Unit> OnDestroyAsObservable()
        {
            if (this == null) return Observable.Return(Unit.Default);
            if (calledDestroy) return Observable.Return(Unit.Default);
            return onDestroy ?? (onDestroy = new Subject<Unit>());
        }
    }
}
