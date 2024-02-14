#if R3_UGUI_SUPPORT
using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableScrollTrigger : ObservableTriggerBase, IEventSystemHandler, IScrollHandler
    {
        Subject<PointerEventData> onScroll;

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (onScroll != null) onScroll.OnNext(eventData);
        }

        public Observable<PointerEventData> OnScrollAsObservable()
        {
            return onScroll ?? (onScroll = new Subject<PointerEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onScroll != null)
            {
                onScroll.OnCompleted();
            }
        }
    }
}

#endif
