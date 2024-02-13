using R3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    [ObservableTrigger(TriggerKinds.Update | TriggerKinds.OnMouseDown)]
    public partial class GeneratorSandbox
    {
    }


    public static class TestExtensions
    {
        public static Observable<Unit> UpdateAsObservable(this GameObject gameObject)
        {
            if (gameObject == null) return Observable.Empty<Unit>();
            return __GetOrAddComponent<ObservableUpdateTrigger>(gameObject).UpdateAsObservable();
        }

        static T __GetOrAddComponent<T>(GameObject gameObject)
            where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
    }

    // [DisallowMultipleComponent]
    public class ObservableUpdateTrigger : Component
    {
        Subject<Unit>? __subject;
        void Update() => __subject?.OnNext(default);
        public Observable<Unit> UpdateAsObservable() => (__subject ??= new Subject<Unit>());
        void OnDestroy() => __subject?.Dispose();
    }

    public class ObservableUpdateTrigger2 : Component
    {
        Subject<Unit>? __subject;
        void Update() => __subject?.OnNext(default);
        public Observable<Unit> UpdateAsObservable() => (__subject ??= new Subject<Unit>());
        void OnDestroy() => __subject?.Dispose();


        
    }

    public class GameObject
    {
        public T GetComponent<T>()
            where T : Component
        {
            throw new NotImplementedException();
        }
        public T AddComponent<T>()
            where T : Component
        {
            throw new NotImplementedException();
        }
    }
}

namespace R3
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ObservableTriggerAttribute : Attribute
    {
        public TriggerKinds Kinds { get; }
        public bool DefineCoreMethod { get; }

        public ObservableTriggerAttribute(TriggerKinds kinds, bool defineCoreMethod = false)
        {
            this.Kinds = kinds;
            this.DefineCoreMethod = defineCoreMethod;
        }
    }
}
