using Microsoft.Xna.Framework;

namespace DarkLight;

public class Camera
{
    public Matrix Transform { get; private set; }
    private Vector2 position;

    public void Update(Player target, int viewportWidth, int viewportHeight)
    {
        var targetPosition = new Vector2(
            target.Position.X + target.Bounds.Width / 2f,
            target.Position.Y + target.Bounds.Height / 2f);
        
        position = Vector2.Lerp(position, targetPosition, 0.084f);

        // Limit camera to map bounds
        // All maps are 30x24 tiles, size of one tile is 128
        var mapWidth = 30 * 128f;
        var mapHeight = 24 * 128f;
        
        var halfViewWidth = viewportWidth / 2f;
        var halfViewHeight = viewportHeight / 2f;

        position.X = MathHelper.Clamp(position.X, halfViewWidth, mapWidth - halfViewWidth);
        position.Y = MathHelper.Clamp(position.Y, halfViewHeight, mapHeight - halfViewHeight);

        Transform = Matrix.CreateTranslation(-position.X, -position.Y, 0) *
                    Matrix.CreateTranslation(halfViewWidth, halfViewHeight, 0);
    }
}
