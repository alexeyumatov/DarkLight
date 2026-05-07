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
    public int ShieldPoints { get; set; } = PlayerData.ShieldValue;
    public int Stamina { get; set; } = 100;

    public bool IsFacingRight { get; private set; } = true;
    public bool IsPoisoned => poisonTimer > 0;
    public bool IsInvincible => invincibilityTimer > 0;

    private readonly Texture2D texture;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, texture.Width, texture.Height);

    private const float MaxMoveSpeed = 500f;
    private const float Acceleration = 4000f;
    private const float Friction = 3500f;
    private const float JumpSpeed = 1100f;
    private const float Gravity = 4500f;
    private const float LadderSpeed = 320f;
    private const float InvincibilityDuration = 1f;

    private bool isGrounded;
    private bool isOnLadder;
    private KeyboardState prevKeyboard;
    private float invincibilityTimer;
    private float poisonTimer;
    private int poisonDps;
    private float poisonAccumulator;

    public Player(Texture2D texture, Vector2 startPosition)
    {
        this.texture = texture;
        Position = startPosition;
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0) return;
        var throughShield = damage - ShieldPoints;
        if (throughShield > 0) { ShieldPoints = 0; HealthPoints -= throughShield; }
        else ShieldPoints -= damage;
        invincibilityTimer = InvincibilityDuration;
    }

    public void ApplyPoison(float duration, int dps)
    {
        poisonTimer = duration;
        poisonDps = dps;
    }

    public void Update(GameTime gameTime, List<Tile> tiles)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (invincibilityTimer > 0) invincibilityTimer -= dt;

        if (poisonTimer > 0)
        {
            poisonTimer -= dt;
            poisonAccumulator += poisonDps * dt;
            int dmg = (int)poisonAccumulator;
            if (dmg > 0) { poisonAccumulator -= dmg; HealthPoints -= dmg; }
        }

        var kb = Keyboard.GetState();

        var touchingLadder = TouchesAny(tiles, t => t.IsLadder);

        // Grab ladder on single E press
        if (!isOnLadder && touchingLadder &&
            kb.IsKeyDown(Keys.E) && !prevKeyboard.IsKeyDown(Keys.E))
        {
            isOnLadder = true;
            Velocity = Vector2.Zero;
        }

        // Auto-release when no ladder tile overlaps
        if (isOnLadder && !touchingLadder)
            isOnLadder = false;

        if (isOnLadder)
            UpdateLadder(kb, dt, tiles);
        else
            UpdateNormal(kb, dt, tiles);

        prevKeyboard = kb;
    }

    private void UpdateLadder(KeyboardState kb, float dt, List<Tile> tiles)
    {
        // Jump off ladder with Space
        if (kb.IsKeyDown(Keys.Space) && !prevKeyboard.IsKeyDown(Keys.Space))
        {
            isOnLadder = false;
            Velocity.Y = -JumpSpeed * 0.6f;
            return;
        }

        var climbDir = 0f;
        if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))   climbDir = -1f;
        if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))  climbDir =  1f;

        var moveDir = 0f;
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
            switch (Velocity.X)
            {
                case > 0f:
                {
                    Velocity.X -= Friction * dt; if (Velocity.X < 0f) Velocity.X = 0f;
                    break;
                }
                case < 0f:
                {
                    Velocity.X += Friction * dt; if (Velocity.X > 0f) Velocity.X = 0f;
                    break;
                }
            }
        }

        Velocity.Y = climbDir * LadderSpeed;

        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, horizontal: true);
        Position.Y += Velocity.Y * dt;
    }

    private void UpdateNormal(KeyboardState kb, float dt, List<Tile> tiles)
    {
        // Horizontal movement
        var moveDir = 0f;
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
            switch (Velocity.X)
            {
                case > 0f:
                {
                    Velocity.X -= Friction * dt; if (Velocity.X < 0f) Velocity.X = 0f;
                    break;
                }
                case < 0f:
                {
                    Velocity.X += Friction * dt; if (Velocity.X > 0f) Velocity.X = 0f;
                    break;
                }
            }
        }

        // Gravity
        Velocity.Y += Gravity * dt;

        // Variable jump height: cut upward speed when Space is released
        if (Velocity.Y < 0 && !(kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.Space)))
            Velocity.Y += (Gravity * dt) / 2f;

        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, horizontal: true);

        Position.Y += Velocity.Y * dt;
        isGrounded = false;
        HandleCollisions(tiles, horizontal: false);

        // Jump — Space or Up arrow only (W removed)
        if (isGrounded && (kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.Up)))
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
                        isGrounded = true;
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
        return tiles.Any(t => predicate(t) && b.Intersects(t.Bounds));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var effect = IsFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, effect, 0f);
    }
}
