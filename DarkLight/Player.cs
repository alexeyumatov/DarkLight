using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DarkLight;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;

    private readonly Texture2D _texture;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
    
    private const float MaxMoveSpeed = 500f;
    private const float Acceleration = 4000f;
    private const float Friction = 3500f;
    private const float JumpSpeed = 1100f;
    private const float Gravity = 4500f;

    private bool _isGrounded;

    public Player(Texture2D texture, Vector2 startPosition)
    {
        _texture = texture;
        Position = startPosition;
    }

    public void Update(GameTime gameTime, System.Collections.Generic.List<Tile> tiles)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var keyboardState = Keyboard.GetState();

        var moveDirection = 0f;
        if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            moveDirection -= 1f;
        if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            moveDirection += 1f;

        if (moveDirection != 0f)
        {
            Velocity.X += moveDirection * Acceleration * dt;
            Velocity.X = MathHelper.Clamp(Velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
        }
        else
        {
            if (Velocity.X > 0f)
            {
                Velocity.X -= Friction * dt;
                if (Velocity.X < 0f) Velocity.X = 0f;
            }
            else if (Velocity.X < 0f)
            {
                Velocity.X += Friction * dt;
                if (Velocity.X > 0f) Velocity.X = 0f;
            }
        }

        Velocity.Y += Gravity * dt;
        
        if (Velocity.Y < 0 && !(keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space)))
        {
            Velocity.Y += (Gravity * dt) / 2;
        }

        // Apply X movement and check collisions
        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, true);

        // Apply Y movement and check collisions
        Position.Y += Velocity.Y * dt;
        _isGrounded = false;
        HandleCollisions(tiles, false);
        
        // Jumping
        if (_isGrounded && (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space)))
        {
            Velocity.Y = -JumpSpeed;
        }
    }

    private void HandleCollisions(System.Collections.Generic.List<Tile> tiles, bool horizontal)
    {
        var playerBounds = Bounds;

        foreach (var tile in tiles.Where(tile => playerBounds.Intersects(tile.Bounds)))
        {
            if (horizontal)
            {
                Position.X = Velocity.X switch
                {
                    > 0 => tile.Bounds.Left - playerBounds.Width, // right
                    < 0 => tile.Bounds.Right, // left
                    _ => Position.X
                };
                Velocity.X = 0f;
            }
            else
            {
                switch (Velocity.Y)
                {
                    // Falling state
                    case > 0:
                        Position.Y = tile.Bounds.Top - playerBounds.Height;
                        _isGrounded = true;
                        break;
                    // Jumping state
                    case < 0:
                        Position.Y = tile.Bounds.Bottom;
                        break;
                }

                Velocity.Y = 0f;
            }

            // Update bounds for the next tile check
            playerBounds = Bounds;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, Position, Color.White);
    }
}
