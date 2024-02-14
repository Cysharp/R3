#if R3_UGUI_SUPPORT
using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableDropTrigger : ObservableTriggerBase, IEventSystemHandler, IDropHandler
    {
        Subject<PointerEventData> onDrop;

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (onDrop != null) onDrop.OnNext(eventData);
        }

        public Observable<PointerEventData> OnDropAsObservable()
        {
            return onDrop ?? (onDrop = new Subject<PointerEventData>());
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onDrop != null)
            {
                onDrop.OnCompleted();
            }
        }
    }
}
#endif
