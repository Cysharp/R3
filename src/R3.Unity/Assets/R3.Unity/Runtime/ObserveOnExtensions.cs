namespace R3 // using R3
{
    public static class ObserveOnExtensions
    {
        public static Observable<T> ObserveOnMainThread<T>(this Observable<T> source)
        {
            return source.ObserveOn(UnityFrameProvider.Update);
        }

        public static Observable<T> SubscribeOnMainThread<T>(this Observable<T> source)
        {
            return source.SubscribeOn(UnityFrameProvider.Update);
        }
    }
}
