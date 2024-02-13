using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableCanvasGroupChangedTrigger : ObservableTriggerBase
    {
        Subject<Unit> onCanvasGroupChanged;

        // Callback that is sent if the canvas group is changed
        void OnCanvasGroupChanged()
        {
            if (onCanvasGroupChanged != null) onCanvasGroupChanged.OnNext(Unit.Default);
        }

        /// <summary>Callback that is sent if the canvas group is changed.</summary>
        public Observable<Unit> OnCanvasGroupChangedAsObservable()
        {
            return onCanvasGroupChanged ?? (onCanvasGroupChanged = new Subject<Unit>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onCanvasGroupChanged != null)
            {
                onCanvasGroupChanged.OnCompleted();
            }
        }
    }
}
