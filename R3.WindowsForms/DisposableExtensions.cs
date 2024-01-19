using System;
using System.ComponentModel;

namespace R3.WindowsForms;

public static class DisposableExtensions
{
    public static void AddTo(this IDisposable disposable, IContainer? container)
    {
        container?.Add(new DisposableWrapper(disposable));
    }

    private sealed class DisposableWrapper(IDisposable disposable) : Component
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposable.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
