#if R3_XRI_SUPPORT
using UnityEngine.XR.Interaction.Toolkit;

namespace R3
{
    public static partial class UnityXRBaseInteractableExtensions
    {
        /// <summary>Observe selectEntered event.</summary>
        public static Observable<SelectEnterEventArgs> OnSelectEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.selectEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe selectExited event.</summary>
        public static Observable<SelectExitEventArgs> OnSelectExitedAsObservable(this XRBaseInteractable component)
        {
            return component.selectExited.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe firstSelectEntered event.</summary>
        public static Observable<SelectEnterEventArgs> OnFirstSelectEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.firstSelectEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe lastSelectExited event.</summary>
        public static Observable<SelectExitEventArgs> OnLastSelectExitedAsObservable(this XRBaseInteractable component)
        {
            return component.lastSelectExited.AsObservable(component.GetDestroyCancellationToken());
        }

        /// <summary>Observe hoverEntered event.</summary>
        public static Observable<HoverEnterEventArgs> OnHoverEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.hoverEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe hoverExited event.</summary>
        public static Observable<HoverExitEventArgs> OnHoverExitedAsObservable(this XRBaseInteractable component)
        {
            return component.hoverExited.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe firstHoverEntered event.</summary>
        public static Observable<HoverEnterEventArgs> OnFirstHoverEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.firstHoverEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe lastHoverExited event.</summary>
        public static Observable<HoverExitEventArgs> OnLastHoverExitedAsObservable(this XRBaseInteractable component)
        {
            return component.lastHoverExited.AsObservable(component.GetDestroyCancellationToken());
        }

        /// <summary>Observe focusEntered event.</summary>
        public static Observable<FocusEnterEventArgs> OnFocusEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.focusEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe focusExited event.</summary>
        public static Observable<FocusExitEventArgs> OnFocusExitedAsObservable(this XRBaseInteractable component)
        {
            return component.focusExited.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe firstFocusEntered event.</summary>
        public static Observable<FocusEnterEventArgs> OnFirstFocusEnteredAsObservable(this XRBaseInteractable component)
        {
            return component.firstFocusEntered.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe lastFocusExited event.</summary>
        public static Observable<FocusExitEventArgs> OnLastFocusExitedAsObservable(this XRBaseInteractable component)
        {
            return component.lastFocusExited.AsObservable(component.GetDestroyCancellationToken());
        }

        /// <summary>Observe activated event.</summary>
        public static Observable<ActivateEventArgs> OnActivatedAsObservable(this XRBaseInteractable component)
        {
            return component.activated.AsObservable(component.GetDestroyCancellationToken());
        }
        /// <summary>Observe activated event.</summary>
        public static Observable<DeactivateEventArgs> OnDeactivatedAsObservable(this XRBaseInteractable component)
        {
            return component.deactivated.AsObservable(component.GetDestroyCancellationToken());
        }
    }
}
#endif
