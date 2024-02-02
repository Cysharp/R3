using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Physics;
using Silk.NET.OpenGL;

namespace R3.Stride.Sandbox
{
    public class CubeCollisionTest : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            // Initialization of the script.
            var component = Entity.Get<RigidbodyComponent>();
            component.AngularVelocity = Vector3.UnitZ;
            component.Collisions.CollectionChangedAsObservable()
                .Subscribe(x =>
                {
                    var (sender, arg) = x;
                    if(arg.Item is Collision collision)
                    {
                        Log.Info($"{sender}: {collision.ColliderA.Entity.Name}, {collision.ColliderB.Entity.Name}, {arg.Action}");
                    }
                    else
                    {
                        Log.Info($"{sender}: {arg.Item}, {arg.Key} {arg.Index}, {arg.OldItem}, {arg.Action}");
                    }
                    component.AngularVelocity = -component.AngularVelocity;
                    //component.UpdatePhysicsTransformation();
                });
            Observable.EveryUpdate()
                .Subscribe(Entity, (_, ent) =>
                {
                    var velocity = component.LinearVelocity;
                    velocity.Z = MathF.Cos((float)Game.UpdateTime.Total.TotalSeconds / 2);
                    component.LinearVelocity = velocity;
                    //component.UpdatePhysicsTransformation();
                    DebugText.Print($"{ent.Transform.Position}", new Int2(10, 300));
                });

        }
    }
}
