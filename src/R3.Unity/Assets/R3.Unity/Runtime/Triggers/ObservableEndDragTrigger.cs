#if R3_UGUI_SUPPORT
using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableEndDragTrigger : ObservableTriggerBase, IEventSystemHandler, IEndDragHandler
    {
        Subject<PointerEventData> onEndDrag;

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag.OnNext(eventData);
        }

        public Observable<PointerEventData> OnEndDragAsObservable()
        {
            return onEndDrag ?? (onEndDrag = new Subject<PointerEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onEndDrag != null)
            {
                onEndDrag.OnCompleted();
            }
        }
    }
}
#endif
