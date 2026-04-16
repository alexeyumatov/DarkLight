using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace DarkLight
{
    public class Tile
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public bool IsCollidable { get; private set; }
        
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        
        public Tile(Texture2D texture, Vector2 position, bool isCollidable = true)
        {
            Texture = texture;
            Position = position;
            IsCollidable = isCollidable;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Position, Color.White);
            }
        }
    }
}
