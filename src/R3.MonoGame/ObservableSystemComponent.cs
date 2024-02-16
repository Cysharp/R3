using System;
using Microsoft.Xna.Framework;

namespace R3;

public class ObservableSystemComponent(Game game, Action<Exception> exceptionHandler) : GameComponent(game)
{
    public ObservableSystemComponent(Game game)
        : this(game, ex => System.Diagnostics.Trace.TraceError("R3 Unhandled Exception {0}", ex))
    {
    }

    public override void Initialize()
    {
        ObservableSystem.RegisterUnhandledExceptionHandler(exceptionHandler);
        ObservableSystem.DefaultTimeProvider = MonoGameTimeProvider.Update;
        ObservableSystem.DefaultFrameProvider = MonoGameFrameProvider.Update;
    }

    public override void Update(GameTime gameTime)
    {
        MonoGameTimeProvider.Update.Tick(gameTime);
        MonoGameFrameProvider.Update.Tick();
    }
}
