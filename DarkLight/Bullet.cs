using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class Bullet
{
    public Vector2 Position;
    public Vector2 Velocity;
    
    private readonly Texture2D texture;
    public int Damage { get; } = PlayerData.BulletDamage;

    private const float Speed = 1000f;
    private int lifeTimeMs = 2000;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 64, 64);

    public bool IsDead => lifeTimeMs <= 0;

    public Bullet(Texture2D texture, Vector2 startPosition, bool isFacingRight)
    {
        this.texture = texture;
        Position = startPosition;
        Velocity = new Vector2(isFacingRight ? Speed : -Speed, 0);
    }

    public void Update(GameTime gameTime, System.Collections.Generic.List<Tile> tiles)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position += Velocity * dt;
        lifeTimeMs -= gameTime.ElapsedGameTime.Milliseconds;

        var bounds = Bounds;
        foreach (var tile in tiles.Where(tile => tile.IsCollidable && bounds.Intersects(tile.Bounds)))
        {
            lifeTimeMs = 0;
            break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var effect = Velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        var destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 64, 64);
        spriteBatch.Draw(texture, destinationRectangle, null, Color.White, 0f, Vector2.Zero, effect, 0f);
    }
}
