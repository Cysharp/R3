#if R3_ANIMATION_SUPPORT
using UnityEngine;

namespace R3.Triggers
{
    public class ObservableStateMachineTrigger : StateMachineBehaviour
    {
        public class OnStateInfo
        {
            public Animator Animator { get; private set; }
            public AnimatorStateInfo StateInfo { get; private set; }
            public int LayerIndex { get; private set; }

            public OnStateInfo(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                Animator = animator;
                StateInfo = stateInfo;
                LayerIndex = layerIndex;
            }
        }

        public class OnStateMachineInfo
        {
            public Animator Animator { get; private set; }
            public int StateMachinePathHash { get; private set; }

            public OnStateMachineInfo(Animator animator, int stateMachinePathHash)
            {
                Animator = animator;
                StateMachinePathHash = stateMachinePathHash;
            }
        }

        // OnStateExit
        Subject<OnStateInfo> onStateExit;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onStateExit?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateExitAsObservable()
        {
            return onStateExit ??= new Subject<OnStateInfo>();
        }

        // OnStateEnter
        Subject<OnStateInfo> onStateEnter;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onStateEnter?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateEnterAsObservable()
        {
            return onStateEnter ??= new Subject<OnStateInfo>();
        }

        // OnStateIK
        Subject<OnStateInfo> onStateIK;

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(onStateIK !=null) onStateIK.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateIKAsObservable()
        {
            return onStateIK ??= new Subject<OnStateInfo>();
        }

        // OnStateUpdate
        Subject<OnStateInfo> onStateUpdate;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onStateUpdate?.OnNext(new OnStateInfo(animator, stateInfo, layerIndex));
        }

        public Observable<OnStateInfo> OnStateUpdateAsObservable()
        {
            return onStateUpdate ??= new Subject<OnStateInfo>();
        }

        // OnStateMachineEnter
        Subject<OnStateMachineInfo> onStateMachineEnter;

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            onStateMachineEnter?.OnNext(new OnStateMachineInfo(animator, stateMachinePathHash));
        }

        public Observable<OnStateMachineInfo> OnStateMachineEnterAsObservable()
        {
            return onStateMachineEnter ??= new Subject<OnStateMachineInfo>();
        }

        // OnStateMachineExit
        Subject<OnStateMachineInfo> onStateMachineExit;

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            onStateMachineExit?.OnNext(new OnStateMachineInfo(animator, stateMachinePathHash));
        }

        public Observable<OnStateMachineInfo> OnStateMachineExitAsObservable()
        {
            return onStateMachineExit ??= new Subject<OnStateMachineInfo>();
        }
    }
}
#endif
