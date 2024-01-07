using UnityEngine.Events;
using UnityEngine.UI;

namespace R3
{
    public static class UnityGraphicExtensions
    {
        public static Observable<Unit> DirtyLayoutCallbackAsObservable(this Graphic graphic)
        {
            return Observable.Create<Unit>(observer =>
            {
                UnityAction registerHandler = () => observer.OnNext(Unit.Default);
                graphic.RegisterDirtyLayoutCallback(registerHandler);
                return Disposable.Create(static state => state.graphic.UnregisterDirtyLayoutCallback(state.registerHandler), (graphic, registerHandler));
            });
        }

        public static Observable<Unit> DirtyMaterialCallbackAsObservable(this Graphic graphic)
        {
            return Observable.Create<Unit>(observer =>
            {
                UnityAction registerHandler = () => observer.OnNext(Unit.Default);
                graphic.RegisterDirtyMaterialCallback(registerHandler);
                return Disposable.Create(static state => state.graphic.UnregisterDirtyMaterialCallback(state.registerHandler), (graphic, registerHandler));
            });
        }

        public static Observable<Unit> DirtyVerticesCallbackAsObservable(this Graphic graphic)
        {
            return Observable.Create<Unit>(observer =>
            {
                UnityAction registerHandler = () => observer.OnNext(Unit.Default);
                graphic.RegisterDirtyVerticesCallback(registerHandler);
                return Disposable.Create(static state => state.graphic.UnregisterDirtyVerticesCallback(state.registerHandler), (graphic, registerHandler));
            });
        }
    }
}
