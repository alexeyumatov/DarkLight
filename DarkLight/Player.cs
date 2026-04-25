using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DarkLight;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;

    public int HealthPoints { get; set; } = 100;
    public int ShieldPoints { get; set; } = 50;
    public int Stamina { get; set; } = 100;

    public bool IsFacingRight { get; private set; } = true;

    private readonly Texture2D _texture;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

    private const float MaxMoveSpeed = 500f;
    private const float Acceleration = 4000f;
    private const float Friction = 3500f;
    private const float JumpSpeed = 1100f;
    private const float Gravity = 4500f;
    private const float LadderSpeed = 320f;

    private bool _isGrounded;
    private bool _isOnLadder;
    private KeyboardState _prevKeyboard;

    public Player(Texture2D texture, Vector2 startPosition)
    {
        _texture = texture;
        Position = startPosition;
    }

    public void Update(GameTime gameTime, List<Tile> tiles)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var kb = Keyboard.GetState();

        bool touchingLadder = TouchesAny(tiles, t => t.IsLadder);

        // Grab ladder on single E press
        if (!_isOnLadder && touchingLadder &&
            kb.IsKeyDown(Keys.E) && !_prevKeyboard.IsKeyDown(Keys.E))
        {
            _isOnLadder = true;
            Velocity = Vector2.Zero;
        }

        // Auto-release when no ladder tile overlaps
        if (_isOnLadder && !touchingLadder)
            _isOnLadder = false;

        if (_isOnLadder)
            UpdateLadder(kb, dt, tiles);
        else
            UpdateNormal(kb, dt, tiles);

        _prevKeyboard = kb;
    }

    private void UpdateLadder(KeyboardState kb, float dt, List<Tile> tiles)
    {
        // Jump off ladder with Space
        if (kb.IsKeyDown(Keys.Space) && !_prevKeyboard.IsKeyDown(Keys.Space))
        {
            _isOnLadder = false;
            Velocity.Y = -JumpSpeed * 0.6f;
            return;
        }

        float climbDir = 0f;
        if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))   climbDir = -1f;
        if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))  climbDir =  1f;

        float moveDir = 0f;
        if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))  moveDir -= 1f;
        if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) moveDir += 1f;

        if (moveDir != 0f)
        {
            Velocity.X += moveDir * Acceleration * dt;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            IsFacingRight = moveDir > 0f;
        }
        else
        {
            if (Velocity.X > 0f) { Velocity.X -= Friction * dt; if (Velocity.X < 0f) Velocity.X = 0f; }
            else if (Velocity.X < 0f) { Velocity.X += Friction * dt; if (Velocity.X > 0f) Velocity.X = 0f; }
        }

        Velocity.Y = climbDir * LadderSpeed;

        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, horizontal: true);
        Position.Y += Velocity.Y * dt;
    }

    private void UpdateNormal(KeyboardState kb, float dt, List<Tile> tiles)
    {
        // Horizontal movement
        float moveDir = 0f;
        if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))  moveDir -= 1f;
        if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) moveDir += 1f;

        if (moveDir != 0f)
        {
            Velocity.X += moveDir * Acceleration * dt;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            IsFacingRight = moveDir > 0f;
        }
        else
        {
            if (Velocity.X > 0f) { Velocity.X -= Friction * dt; if (Velocity.X < 0f) Velocity.X = 0f; }
            else if (Velocity.X < 0f) { Velocity.X += Friction * dt; if (Velocity.X > 0f) Velocity.X = 0f; }
        }

        // Gravity
        Velocity.Y += Gravity * dt;

        // Variable jump height: cut upward speed when Space is released
        if (Velocity.Y < 0 && !(kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.Space)))
            Velocity.Y += (Gravity * dt) / 2f;

        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, horizontal: true);

        Position.Y += Velocity.Y * dt;
        _isGrounded = false;
        HandleCollisions(tiles, horizontal: false);

        // Jump — Space or Up arrow only (W removed)
        if (_isGrounded && (kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.Up)))
            Velocity.Y = -JumpSpeed;
    }

    private void HandleCollisions(List<Tile> tiles, bool horizontal)
    {
        var playerBounds = Bounds;

        foreach (var tile in tiles.Where(tile => tile.IsCollidable && playerBounds.Intersects(tile.Bounds)))
        {
            if (horizontal)
            {
                Position.X = Velocity.X switch
                {
                    > 0 => tile.Bounds.Left - playerBounds.Width,
                    < 0 => tile.Bounds.Right,
                    _ => Position.X
                };
                Velocity.X = 0f;
            }
            else
            {
                switch (Velocity.Y)
                {
                    case > 0:
                        Position.Y = tile.Bounds.Top - playerBounds.Height;
                        _isGrounded = true;
                        break;
                    case < 0:
                        Position.Y = tile.Bounds.Bottom;
                        break;
                }
                Velocity.Y = 0f;
            }

            playerBounds = Bounds;
        }
    }

    private bool TouchesAny(List<Tile> tiles, System.Func<Tile, bool> predicate)
    {
        var b = Bounds;
        foreach (var t in tiles)
            if (predicate(t) && b.Intersects(t.Bounds))
                return true;
        return false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var effect = IsFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(_texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
    }
}
