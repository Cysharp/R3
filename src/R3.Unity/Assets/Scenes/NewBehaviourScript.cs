using R3;
using System;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        Observable.TimerFrame(5, 100)
            .TakeUntil(this.destroyCancellationToken)
            .Subscribe(x =>
            {
                Debug.Log(Time.time);
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(x => true)
            .Subscribe(x =>
            {
                Debug.Log(Time.frameCount);
            })
            .AddTo(this);

        Observable.EveryValueChanged(this, x => x.transform, destroyCancellationToken)
            .Select(x => x)
            .Subscribe(x =>
            {
                Debug.Log(x);
            })
            .AddTo(this);






    }
}
