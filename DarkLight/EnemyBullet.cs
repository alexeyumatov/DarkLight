using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class EnemyBullet
{
    public Vector2 Position;
    public Vector2 Velocity;

    public int Damage { get; }
    public bool IsPoison { get; }

    private readonly Texture2D[] frames;
    private int frameIndex;
    private float frameTimer;
    private float lifetime = 3f;

    private const float FrameSec = 0.1f;
    private const int RenderSize = 30;
    private const float Speed = 400f;

    public bool IsDead => lifetime <= 0;
    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, RenderSize, RenderSize);

    public EnemyBullet(Texture2D[] frames, Vector2 startPos, bool facingRight, int damage, bool isPoison)
    {
        this.frames = frames;
        Position = startPos;
        Velocity = new Vector2(facingRight ? Speed : -Speed, 0);
        Damage = damage;
        IsPoison = isPoison;
    }

    public void Update(GameTime gameTime, List<Tile> tiles)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position += Velocity * dt;
        lifetime -= dt;

        frameTimer += dt;
        if (frameTimer >= FrameSec)
        {
            frameTimer -= FrameSec;
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        var bounds = Bounds;
        foreach (var tile in tiles.Where(t => t.IsCollidable && bounds.Intersects(t.Bounds)))
        {
            lifetime = 0;
            break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var effect = Velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(frames[frameIndex],
            new Rectangle((int)Position.X, (int)Position.Y, RenderSize, RenderSize),
            null, Color.White, 0f, Vector2.Zero, effect, 0f);
    }
}
