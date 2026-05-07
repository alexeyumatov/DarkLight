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
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    // Rendering
    private RenderTarget2D renderTarget;
    private const int LogicalWidth  = 1920;
    private const int LogicalHeight = 1080;
    private float renderScale;
    private int   renderOffsetX;
    private int   renderOffsetY;

    // State
    private GameState gameState = GameState.LevelSelect;
    private LevelSelectScreen levelSelectScreen;
    private MarketScreen      marketScreen;

    // Game objects — null until a level is loaded
    private List<Tile>        tiles        = new();
    private List<Coin>        coins        = new();
    private List<Enemy>       enemies      = new();
    private List<EnemyBullet> enemyBullets = new();
    private Player            player;
    private Camera            camera;
    private HUD               hud;
    private List<Bullet>      bullets      = new();
    private Texture2D         bulletTexture;

    // Input
    private MouseState prevMouse;
    private float      shootCooldown;

    // Music
    private Song menuSong;
    private Song gameSong;
    private Song marketSong;

    private static readonly Color MenuBgColor = new Color(18, 12, 38);

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth  = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        graphics.IsFullScreen = true;
        graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        spriteBatch  = new SpriteBatch(GraphicsDevice);
        renderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);

        float sx = (float)GraphicsDevice.Viewport.Width  / LogicalWidth;
        float sy = (float)GraphicsDevice.Viewport.Height / LogicalHeight;
        renderScale   = Math.Min(sx, sy);
        int drawW      = (int)(LogicalWidth  * renderScale);
        int drawH      = (int)(LogicalHeight * renderScale);
        renderOffsetX = (GraphicsDevice.Viewport.Width  - drawW) / 2;
        renderOffsetY = (GraphicsDevice.Viewport.Height - drawH) / 2;

        levelSelectScreen = new LevelSelectScreen();
        levelSelectScreen.LoadContent(Content, GraphicsDevice);

        marketScreen = new MarketScreen();
        marketScreen.LoadContent(Content, GraphicsDevice);

        hud = new HUD();
        hud.LoadContent(Content);

        bulletTexture = Content.Load<Texture2D>("Hero/Bullet/1");

        menuSong   = Content.Load<Song>("Music/main_menu_melody");
        gameSong   = Content.Load<Song>("Music/game_melody");
        marketSong = Content.Load<Song>("Music/market_melody");

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume      = 0.17f;
        MediaPlayer.Play(menuSong);
    }

    private Point ToLogical(MouseState m) =>
        new((int)((m.X - renderOffsetX) / renderScale),
            (int)((m.Y - renderOffsetY) / renderScale));

    protected override void Update(GameTime gameTime)
    {
        var kb      = Keyboard.GetState();
        var mouse   = Mouse.GetState();
        var logical = ToLogical(mouse);

        bool clicked = mouse.LeftButton == ButtonState.Pressed
                    && prevMouse.LeftButton == ButtonState.Released;

        switch (gameState)
        {
            case GameState.LevelSelect:
                if (kb.IsKeyDown(Keys.Escape)) { Exit(); break; }
                int action = levelSelectScreen.Update(logical, clicked);
                if (action == 0)   EnterMarket();
                else if (action > 0) StartLevel(action);
                break;

            case GameState.Market:
                if (kb.IsKeyDown(Keys.Escape)) { ExitMarket(); break; }
                if (marketScreen.Update(logical, clicked)) ExitMarket();
                break;

            case GameState.Playing:
                if (kb.IsKeyDown(Keys.Escape)) { ReturnToMenu(); break; }
                UpdateGame(gameTime, clicked);
                break;
        }

        prevMouse = mouse;
        base.Update(gameTime);
    }

    private void StartLevel(int number)
    {
        tiles = LevelLoader.LoadTiles(Content, $"Levels/level_{number}.txt",
                                       out Vector2 start, out coins, out enemies);
        var tex = Content.Load<Texture2D>("Hero/HeroStatic/Player_Static_Animation_2");
        player       = new Player(tex, start);
        camera       = new Camera();
        bullets      = new List<Bullet>();
        enemyBullets = new List<EnemyBullet>();
        shootCooldown = 0f;
        gameState     = GameState.Playing;

        MediaPlayer.Stop();
        MediaPlayer.Play(gameSong);
    }

    private void ReturnToMenu()
    {
        tiles.Clear();
        coins.Clear();
        bullets.Clear();
        enemies.Clear();
        enemyBullets.Clear();
        player = null;
        camera = null;
        gameState = GameState.LevelSelect;

        MediaPlayer.Stop();
        MediaPlayer.Play(menuSong);
    }

    private void EnterMarket()
    {
        gameState = GameState.Market;
        MediaPlayer.Stop();
        MediaPlayer.Play(marketSong);
    }

    private void ExitMarket()
    {
        gameState = GameState.LevelSelect;
        MediaPlayer.Stop();
        MediaPlayer.Play(menuSong);
    }

    private void UpdateGame(GameTime gameTime, bool mouseJustPressed)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (shootCooldown > 0) shootCooldown -= dt;

        if (mouseJustPressed && shootCooldown <= 0)
        {
            var startPos = new Vector2(
                player.Position.X + (player.IsFacingRight ? player.Bounds.Width : -bulletTexture.Width),
                player.Position.Y + player.Bounds.Height / 2f - bulletTexture.Height / 2f);
            bullets.Add(new Bullet(bulletTexture, startPos, player.IsFacingRight));
            shootCooldown = PlayerData.BulletCooldown;
        }

        var hit = false;
        for (var i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Update(gameTime, tiles);
            if (bullets[i].IsDead) { bullets.RemoveAt(i); continue; }

            for (var j = enemies.Count - 1; j >= 0; j--)
            {
                if (enemies[j].IsAlive && bullets[i].Bounds.Intersects(enemies[j].Bounds))
                {
                    enemies[j].TakeDamage(bullets[i].Damage);
                    hit = true;
                    break;
                }
            }
            if (hit) bullets.RemoveAt(i);
        }

        // Update enemies, collect spawned enemy bullets
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var eb = enemies[i].Update(gameTime, player, tiles);
            if (eb != null) enemyBullets.Add(eb);
            if (enemies[i].IsFullyDead) enemies.RemoveAt(i);
        }

        // Update enemy bullets, check hits on player
        var pb = player.Bounds;
        for (int i = enemyBullets.Count - 1; i >= 0; i--)
        {
            enemyBullets[i].Update(gameTime, tiles);
            if (enemyBullets[i].IsDead) { enemyBullets.RemoveAt(i); continue; }

            if (enemyBullets[i].Bounds.Intersects(pb))
            {
                player.TakeDamage(enemyBullets[i].Damage);
                if (enemyBullets[i].IsPoison) player.ApplyPoison(5f, 3);
                enemyBullets.RemoveAt(i);
            }
        }

        // Weak enemy contact damage
        pb = player.Bounds;
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive && enemy.Type == EnemyType.Weak && enemy.Bounds.Intersects(pb))
            {
                player.TakeDamage(enemy.ContactDamage);
                break;
            }
        }

        player.Update(gameTime, tiles);
        camera.Update(player, LogicalWidth, LogicalHeight);

        if (player.HealthPoints <= 0) { ReturnToMenu(); return; }

        // Coin collection
        pb = player.Bounds;
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i].TryCollect(pb))
            {
                PlayerData.Coins++;
                coins.RemoveAt(i);
            }
        }

        // Portal — return to level select
        if (tiles.Any(t => t.IsPortal && t.Bounds.Intersects(pb)))
            ReturnToMenu();
    }

    protected override void Draw(GameTime gameTime)
    {
        var logical = ToLogical(prevMouse);

        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(MenuBgColor);

        switch (gameState)
        {
            case GameState.LevelSelect:
                spriteBatch.Begin();
                levelSelectScreen.Draw(spriteBatch, logical);
                spriteBatch.End();
                break;

            case GameState.Market:
                spriteBatch.Begin();
                marketScreen.Draw(spriteBatch, logical);
                spriteBatch.End();
                break;

            case GameState.Playing:
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(transformMatrix: camera.Transform);
                foreach (var tile in tiles)        tile.Draw(spriteBatch);
                foreach (var coin in coins)        coin.Draw(spriteBatch);
                foreach (var enemy in enemies)     enemy.Draw(spriteBatch);
                foreach (var eb in enemyBullets)   eb.Draw(spriteBatch);
                foreach (var bullet in bullets)    bullet.Draw(spriteBatch);
                player.Draw(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin();
                hud.Draw(spriteBatch, player, ultimateAttack: false,
                    heroIsPoisoned: player.IsPoisoned, dashIsReady: true);
                spriteBatch.End();
                break;
        }

        // Scale render target to actual screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        var drawW = (int)(LogicalWidth  * renderScale);
        var drawH = (int)(LogicalHeight * renderScale);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        spriteBatch.Draw(renderTarget,
            new Rectangle(renderOffsetX, renderOffsetY, drawW, drawH), Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }
}
