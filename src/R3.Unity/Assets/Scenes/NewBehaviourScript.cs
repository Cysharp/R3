using R3;
using System;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    IDisposable d;

    void Start()
    {
        var a = Observable.TimerFrame(5, 100)
            .TakeUntil(this.destroyCancellationToken)
            .Subscribe(x =>
            {
                Debug.Log(Time.time);
            });



        var b = Observable.EveryUpdate()
            .Where(x => true)
            .Subscribe(x =>
            {
                Debug.Log(Time.frameCount);
            });

        var c = Observable.EveryValueChanged(this, x => x.transform, destroyCancellationToken)
            .Select(x => x)
            .Subscribe(x =>
            {
                Debug.Log(x);
            });

        d = Disposable.Combine(a, b, c);
    }

    void OnDestroy()
    {
        d.Dispose();
    }
}
