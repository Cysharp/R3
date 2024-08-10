namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> WhereNotNull<TResult>(this Observable<TResult?> source) where TResult : class
    {
        return new WhereSelect<TResult?, TResult>(
            source: source,
            selector: static item => item!,
            predicate: item => item is not null
        );
    }
}
