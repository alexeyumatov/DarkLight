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

    private readonly Texture2D[] _frames;
    private int _frameIndex;
    private float _frameTimer;
    private float _lifetime = 3f;

    private const float FrameSec = 0.1f;
    private const int RenderSize = 30;
    private const float Speed = 400f;

    public bool IsDead => _lifetime <= 0;
    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, RenderSize, RenderSize);

    public EnemyBullet(Texture2D[] frames, Vector2 startPos, bool facingRight, int damage, bool isPoison)
    {
        _frames = frames;
        Position = startPos;
        Velocity = new Vector2(facingRight ? Speed : -Speed, 0);
        Damage = damage;
        IsPoison = isPoison;
    }

    public void Update(GameTime gameTime, List<Tile> tiles)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position += Velocity * dt;
        _lifetime -= dt;

        _frameTimer += dt;
        if (_frameTimer >= FrameSec)
        {
            _frameTimer -= FrameSec;
            _frameIndex = (_frameIndex + 1) % _frames.Length;
        }

        var bounds = Bounds;
        foreach (var tile in tiles.Where(t => t.IsCollidable && bounds.Intersects(t.Bounds)))
        {
            _lifetime = 0;
            break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var effect = Velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(_frames[_frameIndex],
            new Rectangle((int)Position.X, (int)Position.Y, RenderSize, RenderSize),
            null, Color.White, 0f, Vector2.Zero, effect, 0f);
    }
}
