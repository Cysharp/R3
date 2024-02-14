using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableJointTrigger : ObservableTriggerBase
    {
        Subject<float> onJointBreak;

        void OnJointBreak(float breakForce)
        {
            if (onJointBreak != null) onJointBreak.OnNext(breakForce);
        }

        public Observable<float> OnJointBreakAsObservable()
        {
            return onJointBreak ?? (onJointBreak = new Subject<float>());
        }

#if R3_PHYSICS2D_SUPPORT
        
        Subject<Joint2D> onJointBreak2D;

        void OnJointBreak2D(Joint2D brokenJoint)
        {
            if (onJointBreak2D != null) onJointBreak2D.OnNext(brokenJoint);
        }

        public Observable<Joint2D> OnJointBreak2DAsObservable()
        {
            return onJointBreak2D ?? (onJointBreak2D = new Subject<Joint2D>());
        }
#endif

        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onJointBreak != null)
            {
                onJointBreak.OnCompleted();
            }
#if R3_PHYSICS2D_SUPPORT
            if (onJointBreak2D != null)
            {
                onJointBreak2D.OnCompleted();
            }
#endif
        }
    }
}
