using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using R3;

namespace MonoGameApplication1;

class SampleComponent : GameComponent
{
    public SampleComponent(Game game) : base(game)
    {
    }

    public override void Initialize()
    {
        Observable.Interval(TimeSpan.FromMilliseconds(500))
            .GameTime()
            .Subscribe(x =>
            {
                Console.WriteLine($"ElapsedGameTime={x.ElapsedGameTime} TotalGameTime={x.TotalGameTime}");
            });

        Observable.IntervalFrame(10)
            .Subscribe(x =>
            {
                Console.WriteLine($"Frame: {ObservableSystem.DefaultFrameProvider.GetFrameCount()}");
            });
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Components.Add(new ObservableSystemComponent(this));
        Components.Add(new SampleComponent(this));
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
