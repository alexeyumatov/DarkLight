using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class Camera
{
    public Matrix Transform { get; private set; }
    private Vector2 _position;

    public void Update(Player target, int viewportWidth, int viewportHeight)
    {
        var targetPosition = new Vector2(
            target.Position.X + target.Bounds.Width / 2f,
            target.Position.Y + target.Bounds.Height / 2f);
        
        _position = Vector2.Lerp(_position, targetPosition, 0.084f);

        // Limit camera to map bounds
        // All maps are 30x24 tiles, size of one tile is 128
        var mapWidth = 30 * 128f;
        var mapHeight = 24 * 128f;
        
        var halfViewWidth = viewportWidth / 2f;
        var halfViewHeight = viewportHeight / 2f;

        _position.X = MathHelper.Clamp(_position.X, halfViewWidth, mapWidth - halfViewWidth);
        _position.Y = MathHelper.Clamp(_position.Y, halfViewHeight, mapHeight - halfViewHeight);

        Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                    Matrix.CreateTranslation(halfViewWidth, halfViewHeight, 0);
    }
}
