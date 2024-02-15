using System;
using System.Threading;
using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableDestroyTrigger : MonoBehaviour, IFrameRunnerWorkItem
    {
        bool calledDestroy = false;
        Subject<Unit> onDestroy;
        CancellationTokenSource cancellationTokenSource;
        DisposableBag disposableBag;

        bool isMonitoring;

        public bool IsActivated { get; private set; }

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

        public void AddDisposableOnDestroy(IDisposable disposable)
        {
            if (calledDestroy)
            {
                disposable.Dispose();
                return;
            }

            disposableBag.Add(disposable);
        }

        void Awake()
        {
            IsActivated = true;
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        void OnDestroy()
        {
            if (!calledDestroy)
            {
                calledDestroy = true;
                if (cancellationTokenSource != null) cancellationTokenSource.Cancel();
                disposableBag.Dispose();
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

        internal void TryStartActivateMonitoring()
        {
            if (isMonitoring) return;
            isMonitoring = true;
            UnityFrameProvider.Update.Register(this);
        }

        bool IFrameRunnerWorkItem.MoveNext(long frameCount)
        {
            if (IsActivated) return false;

            if (this == null)
            {
                OnDestroy(); // call on destroy manually
                return false;
            }

            // keep monitoring
            return true;
        }
    }
}
