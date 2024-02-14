#if R3_UGUI_SUPPORT
using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservablePointerClickTrigger : ObservableTriggerBase, IEventSystemHandler, IPointerClickHandler
    {
        Subject<PointerEventData> onPointerClick;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (onPointerClick != null) onPointerClick.OnNext(eventData);
        }

        public Observable<PointerEventData> OnPointerClickAsObservable()
        {
            return onPointerClick ?? (onPointerClick = new Subject<PointerEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onPointerClick != null)
            {
                onPointerClick.OnCompleted();
            }
        }
    }
}
#endif
