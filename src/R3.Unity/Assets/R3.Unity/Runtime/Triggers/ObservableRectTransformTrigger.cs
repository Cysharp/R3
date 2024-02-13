using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableRectTransformTrigger : ObservableTriggerBase
    {
        Subject<Unit> onRectTransformDimensionsChange;

        // Callback that is sent if an associated RectTransform has it's dimensions changed
        void OnRectTransformDimensionsChange()
        {
            if (onRectTransformDimensionsChange != null) onRectTransformDimensionsChange.OnNext(Unit.Default);
        }

        /// <summary>Callback that is sent if an associated RectTransform has it's dimensions changed.</summary>
        public Observable<Unit> OnRectTransformDimensionsChangeAsObservable()
        {
            return onRectTransformDimensionsChange ?? (onRectTransformDimensionsChange = new Subject<Unit>());
        }

        Subject<Unit> onRectTransformRemoved;

        // Callback that is sent if an associated RectTransform is removed
        void OnRectTransformRemoved()
        {
            if (onRectTransformRemoved != null) onRectTransformRemoved.OnNext(Unit.Default);
        }

        /// <summary>Callback that is sent if an associated RectTransform is removed.</summary>
        public Observable<Unit> OnRectTransformRemovedAsObservable()
        {
            return onRectTransformRemoved ?? (onRectTransformRemoved = new Subject<Unit>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onRectTransformDimensionsChange != null)
            {
                onRectTransformDimensionsChange.OnCompleted();
            }
            if (onRectTransformRemoved != null)
            {
                onRectTransformRemoved.OnCompleted();
            }
        }

    }
}
