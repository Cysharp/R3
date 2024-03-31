
using UnityEngine;

#if R3_UGUI_SUPPORT
using UnityEngine.EventSystems;
#endif

namespace R3.Triggers
{
    // for Component
    public static partial class ObservableTriggerExtensions
    {
        #region ObservableAnimatorTrigger

        /// <summary>Callback for setting up animation IK (inverse kinematics).</summary>
        public static Observable<int> OnAnimatorIKAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<int>();
            return GetOrAddComponent<ObservableAnimatorTrigger>(component.gameObject).OnAnimatorIKAsObservable();
        }

        /// <summary>Callback for processing animation movements for modifying root motion.</summary>
        public static Observable<Unit> OnAnimatorMoveAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableAnimatorTrigger>(component.gameObject).OnAnimatorMoveAsObservable();
        }

        #endregion

#region ObservableCollision2DTrigger
#if R3_PHYSICS2D_SUPPORT
        /// <summary>Sent when an incoming collider makes contact with this object's collider (2D physics only).</summary>
        public static Observable<Collision2D> OnCollisionEnter2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision2D>();
            return GetOrAddComponent<ObservableCollision2DTrigger>(component.gameObject).OnCollisionEnter2DAsObservable();
        }


        /// <summary>Sent when a collider on another object stops touching this object's collider (2D physics only).</summary>
        public static Observable<Collision2D> OnCollisionExit2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision2D>();
            return GetOrAddComponent<ObservableCollision2DTrigger>(component.gameObject).OnCollisionExit2DAsObservable();
        }

        /// <summary>Sent each frame where a collider on another object is touching this object's collider (2D physics only).</summary>
        public static Observable<Collision2D> OnCollisionStay2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision2D>();
            return GetOrAddComponent<ObservableCollision2DTrigger>(component.gameObject).OnCollisionStay2DAsObservable();
        }
#endif
#endregion

#region ObservableCollisionTrigger
#if R3_PHYSICS_SUPPORT

        /// <summary>OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.</summary>
        public static Observable<Collision> OnCollisionEnterAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision>();
            return GetOrAddComponent<ObservableCollisionTrigger>(component.gameObject).OnCollisionEnterAsObservable();
        }


        /// <summary>OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.</summary>
        public static Observable<Collision> OnCollisionExitAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision>();
            return GetOrAddComponent<ObservableCollisionTrigger>(component.gameObject).OnCollisionExitAsObservable();
        }

        /// <summary>OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.</summary>
        public static Observable<Collision> OnCollisionStayAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collision>();
            return GetOrAddComponent<ObservableCollisionTrigger>(component.gameObject).OnCollisionStayAsObservable();
        }
#endif
#endregion

        #region ObservableDestroyTrigger

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public static Observable<Unit> OnDestroyAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Return(Unit.Default); // send destroy message
            return GetOrAddComponent<ObservableDestroyTrigger>(component.gameObject).OnDestroyAsObservable();
        }

        #endregion


        #region ObservableEnableTrigger

        /// <summary>This function is called when the object becomes enabled and active.</summary>
        public static Observable<Unit> OnEnableAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableEnableTrigger>(component.gameObject).OnEnableAsObservable();
        }

        /// <summary>This function is called when the behaviour becomes disabled () or inactive.</summary>
        public static Observable<Unit> OnDisableAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableEnableTrigger>(component.gameObject).OnDisableAsObservable();
        }

        #endregion

        #region ObservableFixedUpdateTrigger

        /// <summary>This function is called every fixed framerate frame, if the MonoBehaviour is enabled.</summary>
        public static Observable<Unit> FixedUpdateAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableFixedUpdateTrigger>(component.gameObject).FixedUpdateAsObservable();
        }

        #endregion

        #region ObservableLateUpdateTrigger

        /// <summary>LateUpdate is called every frame, if the Behaviour is enabled.</summary>
        public static Observable<Unit> LateUpdateAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableLateUpdateTrigger>(component.gameObject).LateUpdateAsObservable();
        }

        #endregion

#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_METRO)

        #region ObservableMouseTrigger

        /// <summary>OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.</summary>
        public static Observable<Unit> OnMouseDownAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseDownAsObservable();
        }

        /// <summary>OnMouseDrag is called when the user has clicked on a GUIElement or Collider and is still holding down the mouse.</summary>
        public static Observable<Unit> OnMouseDragAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseDragAsObservable();
        }

        /// <summary>OnMouseEnter is called when the mouse entered the GUIElement or Collider.</summary>
        public static Observable<Unit> OnMouseEnterAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseEnterAsObservable();
        }

        /// <summary>OnMouseExit is called when the mouse is not any longer over the GUIElement or Collider.</summary>
        public static Observable<Unit> OnMouseExitAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseExitAsObservable();
        }

        /// <summary>OnMouseOver is called every frame while the mouse is over the GUIElement or Collider.</summary>
        public static Observable<Unit> OnMouseOverAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseOverAsObservable();
        }

        /// <summary>OnMouseUp is called when the user has released the mouse button.</summary>
        public static Observable<Unit> OnMouseUpAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseUpAsObservable();
        }

        /// <summary>OnMouseUpAsButton is only called when the mouse is released over the same GUIElement or Collider as it was pressed.</summary>
        public static Observable<Unit> OnMouseUpAsButtonAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableMouseTrigger>(component.gameObject).OnMouseUpAsButtonAsObservable();
        }

        #endregion

#endif

#region ObservableTrigger2DTrigger
#if R3_PHYSICS2D_SUPPORT
        /// <summary>Sent when another object enters a trigger collider attached to this object (2D physics only).</summary>
        public static Observable<Collider2D> OnTriggerEnter2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider2D>();
            return GetOrAddComponent<ObservableTrigger2DTrigger>(component.gameObject).OnTriggerEnter2DAsObservable();
        }


        /// <summary>Sent when another object leaves a trigger collider attached to this object (2D physics only).</summary>
        public static Observable<Collider2D> OnTriggerExit2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider2D>();
            return GetOrAddComponent<ObservableTrigger2DTrigger>(component.gameObject).OnTriggerExit2DAsObservable();
        }

        /// <summary>Sent each frame where another object is within a trigger collider attached to this object (2D physics only).</summary>
        public static Observable<Collider2D> OnTriggerStay2DAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider2D>();
            return GetOrAddComponent<ObservableTrigger2DTrigger>(component.gameObject).OnTriggerStay2DAsObservable();
        }
#endif
#endregion

#region ObservableTriggerTrigger
#if R3_PHYSICS_SUPPORT

        /// <summary>OnTriggerEnter is called when the Collider other enters the trigger.</summary>
        public static Observable<Collider> OnTriggerEnterAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider>();
            return GetOrAddComponent<ObservableTriggerTrigger>(component.gameObject).OnTriggerEnterAsObservable();
        }


        /// <summary>OnTriggerExit is called when the Collider other has stopped touching the trigger.</summary>
        public static Observable<Collider> OnTriggerExitAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider>();
            return GetOrAddComponent<ObservableTriggerTrigger>(component.gameObject).OnTriggerExitAsObservable();
        }

        /// <summary>OnTriggerStay is called once per frame for every Collider other that is touching the trigger.</summary>
        public static Observable<Collider> OnTriggerStayAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Collider>();
            return GetOrAddComponent<ObservableTriggerTrigger>(component.gameObject).OnTriggerStayAsObservable();
        }
#endif
#endregion

        #region ObservableUpdateTrigger

        /// <summary>Update is called every frame, if the MonoBehaviour is enabled.</summary>
        public static Observable<Unit> UpdateAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableUpdateTrigger>(component.gameObject).UpdateAsObservable();
        }

        #endregion

        #region ObservableVisibleTrigger

        /// <summary>OnBecameInvisible is called when the renderer is no longer visible by any camera.</summary>
        public static Observable<Unit> OnBecameInvisibleAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableVisibleTrigger>(component.gameObject).OnBecameInvisibleAsObservable();
        }

        /// <summary>OnBecameVisible is called when the renderer became visible by any camera.</summary>
        public static Observable<Unit> OnBecameVisibleAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableVisibleTrigger>(component.gameObject).OnBecameVisibleAsObservable();
        }

        #endregion

        #region ObservableTransformChangedTrigger

        /// <summary>Callback sent to the graphic before a Transform parent change occurs.</summary>
        public static Observable<Unit> OnBeforeTransformParentChangedAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableTransformChangedTrigger>(component.gameObject).OnBeforeTransformParentChangedAsObservable();
        }

        /// <summary>This function is called when the parent property of the transform of the GameObject has changed.</summary>
        public static Observable<Unit> OnTransformParentChangedAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableTransformChangedTrigger>(component.gameObject).OnTransformParentChangedAsObservable();
        }

        /// <summary>This function is called when the list of children of the transform of the GameObject has changed.</summary>
        public static Observable<Unit> OnTransformChildrenChangedAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableTransformChangedTrigger>(component.gameObject).OnTransformChildrenChangedAsObservable();
        }

        #endregion

        #region ObservableCanvasGroupChangedTrigger

        /// <summary>Callback that is sent if the canvas group is changed.</summary>
        public static Observable<Unit> OnCanvasGroupChangedAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableCanvasGroupChangedTrigger>(component.gameObject).OnCanvasGroupChangedAsObservable();
        }

        #endregion

        #region ObservableRectTransformTrigger

        /// <summary>Callback that is sent if an associated RectTransform has it's dimensions changed.</summary>
        public static Observable<Unit> OnRectTransformDimensionsChangeAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableRectTransformTrigger>(component.gameObject).OnRectTransformDimensionsChangeAsObservable();
        }

        /// <summary>Callback that is sent if an associated RectTransform is removed.</summary>
        public static Observable<Unit> OnRectTransformRemovedAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableRectTransformTrigger>(component.gameObject).OnRectTransformRemovedAsObservable();
        }

        #endregion

        // uGUI

        #region ObservableEventTrigger classes
#if R3_UGUI_SUPPORT
        public static Observable<BaseEventData> OnDeselectAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<BaseEventData>();
            return GetOrAddComponent<ObservableDeselectTrigger>(component.gameObject).OnDeselectAsObservable();
        }

        public static Observable<AxisEventData> OnMoveAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<AxisEventData>();
            return GetOrAddComponent<ObservableMoveTrigger>(component.gameObject).OnMoveAsObservable();
        }

        public static Observable<PointerEventData> OnPointerDownAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservablePointerDownTrigger>(component.gameObject).OnPointerDownAsObservable();
        }

        public static Observable<PointerEventData> OnPointerEnterAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservablePointerEnterTrigger>(component.gameObject).OnPointerEnterAsObservable();
        }

        public static Observable<PointerEventData> OnPointerExitAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservablePointerExitTrigger>(component.gameObject).OnPointerExitAsObservable();
        }

        public static Observable<PointerEventData> OnPointerUpAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservablePointerUpTrigger>(component.gameObject).OnPointerUpAsObservable();
        }

        public static Observable<BaseEventData> OnSelectAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<BaseEventData>();
            return GetOrAddComponent<ObservableSelectTrigger>(component.gameObject).OnSelectAsObservable();
        }

        public static Observable<PointerEventData> OnPointerClickAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservablePointerClickTrigger>(component.gameObject).OnPointerClickAsObservable();
        }

        public static Observable<BaseEventData> OnSubmitAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<BaseEventData>();
            return GetOrAddComponent<ObservableSubmitTrigger>(component.gameObject).OnSubmitAsObservable();
        }

        public static Observable<PointerEventData> OnDragAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableDragTrigger>(component.gameObject).OnDragAsObservable();
        }

        public static Observable<PointerEventData> OnBeginDragAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableBeginDragTrigger>(component.gameObject).OnBeginDragAsObservable();
        }

        public static Observable<PointerEventData> OnEndDragAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableEndDragTrigger>(component.gameObject).OnEndDragAsObservable();
        }

        public static Observable<PointerEventData> OnDropAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableDropTrigger>(component.gameObject).OnDropAsObservable();
        }

        public static Observable<BaseEventData> OnUpdateSelectedAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<BaseEventData>();
            return GetOrAddComponent<ObservableUpdateSelectedTrigger>(component.gameObject).OnUpdateSelectedAsObservable();
        }

        public static Observable<PointerEventData> OnInitializePotentialDragAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableInitializePotentialDragTrigger>(component.gameObject).OnInitializePotentialDragAsObservable();
        }

        public static Observable<BaseEventData> OnCancelAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<BaseEventData>();
            return GetOrAddComponent<ObservableCancelTrigger>(component.gameObject).OnCancelAsObservable();
        }

        public static Observable<PointerEventData> OnScrollAsObservable(this UIBehaviour component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<PointerEventData>();
            return GetOrAddComponent<ObservableScrollTrigger>(component.gameObject).OnScrollAsObservable();
        }
#endif
        #endregion

        #region ObservableParticleTrigger

        /// <summary>OnParticleCollision is called when a particle hits a collider.</summary>
        public static Observable<GameObject> OnParticleCollisionAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<GameObject>();
            return GetOrAddComponent<ObservableParticleTrigger>(component.gameObject).OnParticleCollisionAsObservable();
        }

#if UNITY_5_4_OR_NEWER

        /// <summary>OnParticleTrigger is called when any particles in a particle system meet the conditions in the trigger module.</summary>
        public static Observable<Unit> OnParticleTriggerAsObservable(this Component component)
        {
            if (component == null || component.gameObject == null) return Observable.Empty<Unit>();
            return GetOrAddComponent<ObservableParticleTrigger>(component.gameObject).OnParticleTriggerAsObservable();
        }

#endif

        #endregion
    }
}
