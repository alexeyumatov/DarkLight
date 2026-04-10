using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DarkLight;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private List<Tile> _tiles = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _tiles = LevelLoader.LoadTiles(Content, "Levels/level_1.txt");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        foreach (var tile in _tiles)
        {
            tile.Draw(_spriteBatch);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}