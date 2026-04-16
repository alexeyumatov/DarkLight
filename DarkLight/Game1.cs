﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DarkLight;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private List<Tile> _tiles = new();
    private Player _player;
    private Camera _camera;
    private HUD _hud;

    private RenderTarget2D _renderTarget;
    private const int LogicalWidth = 1920;
    private const int LogicalHeight = 1080;

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
        _renderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
        _camera = new Camera();
        _hud = new HUD();
        _hud.LoadContent(Content);
        
        Vector2 startPosition;
        _tiles = LevelLoader.LoadTiles(Content, "Levels/level_1.txt", out startPosition);

        var playerTexture = Content.Load<Texture2D>("Hero/HeroStatic/Player_Static_Animation_2");
        _player = new Player(playerTexture, startPosition);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        _player.Update(gameTime, _tiles);
        _camera.Update(_player, LogicalWidth, LogicalHeight);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(transformMatrix: _camera.Transform);
        foreach (var tile in _tiles)
        {
            tile.Draw(_spriteBatch);
        }
        
        _player.Draw(_spriteBatch);
        
        _spriteBatch.End();

        // Draw UI
        _spriteBatch.Begin();
        _hud.Draw(_spriteBatch, _player, ultimateAttack: false, heroIsPoisoned: false, dashIsReady: true);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        float scaleX = (float)GraphicsDevice.Viewport.Width / LogicalWidth;
        float scaleY = (float)GraphicsDevice.Viewport.Height / LogicalHeight;
        float scale = System.Math.Min(scaleX, scaleY);
        
        int drawWidth = (int)(LogicalWidth * scale);
        int drawHeight = (int)(LogicalHeight * scale);
        int destX = (GraphicsDevice.Viewport.Width - drawWidth) / 2;
        int destY = (GraphicsDevice.Viewport.Height - drawHeight) / 2;

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        _spriteBatch.Draw(_renderTarget, new Rectangle(destX, destY, drawWidth, drawHeight), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}