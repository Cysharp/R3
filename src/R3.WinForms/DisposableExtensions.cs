using System;
using System.ComponentModel;

namespace R3.WinForms;

public static class DisposableExtensions
{
    /// <summary>
    /// Destroy objects that implement <see cref="IDisposable"/> in sync with <see cref="IContainer"/>.
    /// </summary>
    /// <param name="disposable">Objects to be disposed</param>
    /// <param name="container">Container to manage object lifetime</param>
    /// <remarks>If a form does not have a member derived from a <seealso cref="Component"/> (such as <see cref="System.Windows.Forms.Timer"/>, but not <see cref="System.Windows.Forms.Control"/>), the container is not created by the form designer.</remarks>
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
