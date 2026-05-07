using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class Coin
{
    private readonly Texture2D _texture;
    public Vector2 Position { get; }
    public bool IsCollected { get; private set; }

    // Display rect: coin scaled 4× (40×60) and centered inside a 128×128 tile cell.
    private Rectangle DrawRect => new(
        (int)Position.X + (LevelLoader.TileSize - 40) / 2,
        (int)Position.Y + (LevelLoader.TileSize - 60) / 2,
        40, 60);

    // Collection hitbox — the full tile cell for comfortable pickup.
    public Rectangle CollectRect => new(
        (int)Position.X, (int)Position.Y,
        LevelLoader.TileSize, LevelLoader.TileSize);

    public Coin(Texture2D texture, Vector2 position)
    {
        _texture = texture;
        Position = position;
    }

    // Returns true if the player touched the coin for the first time.
    public bool TryCollect(Rectangle playerBounds)
    {
        if (!IsCollected && CollectRect.Intersects(playerBounds))
        {
            IsCollected = true;
            return true;
        }
        return false;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsCollected)
            spriteBatch.Draw(_texture, DrawRect, Color.White);
    }
}
