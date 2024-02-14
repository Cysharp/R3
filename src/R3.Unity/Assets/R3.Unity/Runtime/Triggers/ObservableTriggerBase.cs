
using UnityEngine;

namespace R3.Triggers
{
    public abstract class ObservableTriggerBase : MonoBehaviour
    {
        //bool calledAwake = false;
        //Subject<Unit> awake;

        ///// <summary>Awake is called when the script instance is being loaded.</summary>
        //void Awake()
        //{
        //    calledAwake = true;
        //    if (awake != null) { awake.OnNext(Unit.Default); awake.OnCompleted(); }
        //}

        ///// <summary>Awake is called when the script instance is being loaded.</summary>
        //public Observable<Unit> AwakeAsObservable()
        //{
        //    if (calledAwake) return Observable.Return(Unit.Default);
        //    return awake ?? (awake = new Subject<Unit>());
        //}

        //bool calledStart = false;
        //Subject<Unit> start;

        ///// <summary>Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
        //void Start()
        //{
        //    calledStart = true;
        //    if (start != null) { start.OnNext(Unit.Default); start.OnCompleted(); }
        //}

        ///// <summary>Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.</summary>
        //public Observable<Unit> StartAsObservable()
        //{
        //    if (calledStart) return Observable.Return(Unit.Default);
        //    return start ?? (start = new Subject<Unit>());
        //}


        bool calledDestroy = false;

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        void OnDestroy()
        {
            if (!calledDestroy)
            {
                calledDestroy = true;
                RaiseOnCompletedOnDestroy();
            }
        }

        ///// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        //public Observable<Unit> OnDestroyAsObservable()
        //{
        //    if (this == null) return Observable.Return(Unit.Default);
        //    if (calledDestroy) return Observable.Return(Unit.Default);
        //    return onDestroy ?? (onDestroy = new Subject<Unit>());
        //}

        protected abstract void RaiseOnCompletedOnDestroy();
    }
}
