#if R3_UGUI_SUPPORT

using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableBeginDragTrigger : ObservableTriggerBase, IEventSystemHandler, IBeginDragHandler
    {
        Subject<PointerEventData> onBeginDrag;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag.OnNext(eventData);
        }

        public Observable<PointerEventData> OnBeginDragAsObservable()
        {
            return onBeginDrag ?? (onBeginDrag = new Subject<PointerEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onBeginDrag != null)
            {
                onBeginDrag.OnCompleted();
            }
        }
    }
}

#endif
