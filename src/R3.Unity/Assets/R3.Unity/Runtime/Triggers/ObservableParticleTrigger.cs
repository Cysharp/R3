using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableParticleTrigger : ObservableTriggerBase
    {
        Subject<GameObject> onParticleCollision;
        Subject<Unit> onParticleTrigger;

        /// <summary>OnParticleCollision is called when a particle hits a collider.</summary>
        void OnParticleCollision(GameObject other)
        {
            if (onParticleCollision != null) onParticleCollision.OnNext(other);
        }

        /// <summary>OnParticleCollision is called when a particle hits a collider.</summary>
        public Observable<GameObject> OnParticleCollisionAsObservable()
        {
            return onParticleCollision ?? (onParticleCollision = new Subject<GameObject>());
        }

        /// <summary>OnParticleTrigger is called when any particles in a particle system meet the conditions in the trigger module.</summary>
        void OnParticleTrigger()
        {
            if (onParticleTrigger != null) onParticleTrigger.OnNext(Unit.Default);
        }

        /// <summary>OnParticleTrigger is called when any particles in a particle system meet the conditions in the trigger module.</summary>
        public Observable<Unit> OnParticleTriggerAsObservable()
        {
            return onParticleTrigger ?? (onParticleTrigger = new Subject<Unit>());
        }


        protected override void RaiseOnCompletedOnDestroy()
        {
            if (onParticleCollision != null)
            {
                onParticleCollision.OnCompleted();
            }
            if (onParticleTrigger != null)
            {
                onParticleTrigger.OnCompleted();
            }
        }
    }
}
