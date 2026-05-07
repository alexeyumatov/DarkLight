using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DarkLight;

public enum GameState { LevelSelect, Market, Playing }

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Rendering
    private RenderTarget2D _renderTarget;
    private const int LogicalWidth  = 1920;
    private const int LogicalHeight = 1080;
    private float _renderScale;
    private int   _renderOffsetX;
    private int   _renderOffsetY;

    // State
    private GameState _gameState = GameState.LevelSelect;
    private LevelSelectScreen _levelSelectScreen;
    private MarketScreen      _marketScreen;

    // Game objects — null until a level is loaded
    private List<Tile>   _tiles   = new();
    private List<Coin>   _coins   = new();
    private Player       _player;
    private Camera       _camera;
    private HUD          _hud;
    private List<Bullet> _bullets = new();
    private Texture2D    _bulletTexture;

    // Input
    private MouseState _prevMouse;
    private float      _shootCooldown;

    // Music
    private Song _menuSong;
    private Song _gameSong;
    private Song _marketSong;

    private static readonly Color MenuBgColor = new Color(18, 12, 38);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth  = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch  = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);

        float sx = (float)GraphicsDevice.Viewport.Width  / LogicalWidth;
        float sy = (float)GraphicsDevice.Viewport.Height / LogicalHeight;
        _renderScale   = Math.Min(sx, sy);
        int drawW      = (int)(LogicalWidth  * _renderScale);
        int drawH      = (int)(LogicalHeight * _renderScale);
        _renderOffsetX = (GraphicsDevice.Viewport.Width  - drawW) / 2;
        _renderOffsetY = (GraphicsDevice.Viewport.Height - drawH) / 2;

        _levelSelectScreen = new LevelSelectScreen();
        _levelSelectScreen.LoadContent(Content, GraphicsDevice);

        _marketScreen = new MarketScreen();
        _marketScreen.LoadContent(Content, GraphicsDevice);

        _hud = new HUD();
        _hud.LoadContent(Content);

        _bulletTexture = Content.Load<Texture2D>("Hero/Bullet/1");

        _menuSong   = Content.Load<Song>("Music/main_menu_melody");
        _gameSong   = Content.Load<Song>("Music/game_melody");
        _marketSong = Content.Load<Song>("Music/market_melody");

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume      = 0.17f;
        MediaPlayer.Play(_menuSong);
    }

    private Point ToLogical(MouseState m) =>
        new((int)((m.X - _renderOffsetX) / _renderScale),
            (int)((m.Y - _renderOffsetY) / _renderScale));

    protected override void Update(GameTime gameTime)
    {
        var kb      = Keyboard.GetState();
        var mouse   = Mouse.GetState();
        var logical = ToLogical(mouse);

        bool clicked = mouse.LeftButton == ButtonState.Pressed
                    && _prevMouse.LeftButton == ButtonState.Released;

        switch (_gameState)
        {
            case GameState.LevelSelect:
                if (kb.IsKeyDown(Keys.Escape)) { Exit(); break; }
                int action = _levelSelectScreen.Update(logical, clicked);
                if (action == 0)   EnterMarket();
                else if (action > 0) StartLevel(action);
                break;

            case GameState.Market:
                if (kb.IsKeyDown(Keys.Escape)) { ExitMarket(); break; }
                if (_marketScreen.Update(logical, clicked)) ExitMarket();
                break;

            case GameState.Playing:
                if (kb.IsKeyDown(Keys.Escape)) { ReturnToMenu(); break; }
                UpdateGame(gameTime, clicked);
                break;
        }

        _prevMouse = mouse;
        base.Update(gameTime);
    }

    private void StartLevel(int number)
    {
        _tiles = LevelLoader.LoadTiles(Content, $"Levels/level_{number}.txt",
                                       out Vector2 start, out _coins);
        var tex = Content.Load<Texture2D>("Hero/HeroStatic/Player_Static_Animation_2");
        _player  = new Player(tex, start);
        _camera  = new Camera();
        _bullets = new List<Bullet>();
        _shootCooldown = 0f;
        _gameState     = GameState.Playing;

        MediaPlayer.Stop();
        MediaPlayer.Play(_gameSong);
    }

    private void ReturnToMenu()
    {
        _tiles.Clear();
        _coins.Clear();
        _bullets.Clear();
        _player = null;
        _camera = null;
        _gameState = GameState.LevelSelect;

        MediaPlayer.Stop();
        MediaPlayer.Play(_menuSong);
    }

    private void EnterMarket()
    {
        _gameState = GameState.Market;
        MediaPlayer.Stop();
        MediaPlayer.Play(_marketSong);
    }

    private void ExitMarket()
    {
        _gameState = GameState.LevelSelect;
        MediaPlayer.Stop();
        MediaPlayer.Play(_menuSong);
    }

    private void UpdateGame(GameTime gameTime, bool mouseJustPressed)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_shootCooldown > 0) _shootCooldown -= dt;

        if (mouseJustPressed && _shootCooldown <= 0)
        {
            var startPos = new Vector2(
                _player.Position.X + (_player.IsFacingRight ? _player.Bounds.Width : -_bulletTexture.Width),
                _player.Position.Y + _player.Bounds.Height / 2f - _bulletTexture.Height / 2f);
            _bullets.Add(new Bullet(_bulletTexture, startPos, _player.IsFacingRight));
            _shootCooldown = PlayerData.BulletCooldown;
        }

        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            _bullets[i].Update(gameTime, _tiles);
            if (_bullets[i].IsDead) _bullets.RemoveAt(i);
        }

        _player.Update(gameTime, _tiles);
        _camera.Update(_player, LogicalWidth, LogicalHeight);

        // Coin collection
        var pb = _player.Bounds;
        for (int i = _coins.Count - 1; i >= 0; i--)
        {
            if (_coins[i].TryCollect(pb))
            {
                PlayerData.Coins++;
                _coins.RemoveAt(i);
            }
        }

        // Portal — return to level select
        if (_tiles.Any(t => t.IsPortal && t.Bounds.Intersects(pb)))
            ReturnToMenu();
    }

    protected override void Draw(GameTime gameTime)
    {
        var logical = ToLogical(_prevMouse);

        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(MenuBgColor);

        switch (_gameState)
        {
            case GameState.LevelSelect:
                _spriteBatch.Begin();
                _levelSelectScreen.Draw(_spriteBatch, logical);
                _spriteBatch.End();
                break;

            case GameState.Market:
                _spriteBatch.Begin();
                _marketScreen.Draw(_spriteBatch, logical);
                _spriteBatch.End();
                break;

            case GameState.Playing:
                GraphicsDevice.Clear(Color.Black);

                _spriteBatch.Begin(transformMatrix: _camera.Transform);
                foreach (var tile in _tiles)   tile.Draw(_spriteBatch);
                foreach (var coin in _coins)   coin.Draw(_spriteBatch);
                foreach (var bullet in _bullets) bullet.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);
                _spriteBatch.End();

                _spriteBatch.Begin();
                _hud.Draw(_spriteBatch, _player, ultimateAttack: false, heroIsPoisoned: false, dashIsReady: true);
                _spriteBatch.End();
                break;
        }

        // Scale render target to actual screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        int drawW = (int)(LogicalWidth  * _renderScale);
        int drawH = (int)(LogicalHeight * _renderScale);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        _spriteBatch.Draw(_renderTarget,
            new Rectangle(_renderOffsetX, _renderOffsetY, drawW, drawH), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
