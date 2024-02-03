using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Engine.Events;

namespace R3.Stride.Sandbox;
public class EventKeyReceiverTest : StartupScript
{
    EventKey<int> EventKey1 = new EventKey<int>("EventKeyTest", "Event1");
    EventKey EventKey2 = new EventKey("EventKeyTest", "Event2");
    public override void Start()
    {
        EventKey1.AsObservable()
            .Subscribe(x =>
            {
                Log.Info($"event1 published: {x}");
            });
        EventKey2.AsObservable()
            .Subscribe(_ =>
            {
                Log.Info("event2 published");
            });
        Observable.EveryUpdate()
            .ThrottleLastFrame(60)
            .Select((_, i) => i)
            .Subscribe(x =>
            {
                EventKey1.Broadcast(x);
                EventKey2.Broadcast();
            });
    }
}
