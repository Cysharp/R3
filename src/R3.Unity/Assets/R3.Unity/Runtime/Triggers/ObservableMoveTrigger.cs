#if R3_UGUI_SUPPORT
using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableMoveTrigger : ObservableTriggerBase, IEventSystemHandler, IMoveHandler
    {
        Subject<AxisEventData> onMove;

        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            if (onMove != null) onMove.OnNext(eventData);
        }

        public Observable<AxisEventData> OnMoveAsObservable()
        {
            return onMove ?? (onMove = new Subject<AxisEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onMove != null)
            {
                onMove.OnCompleted();
            }
        }
    }
}
#endif
