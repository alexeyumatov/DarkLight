using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class LevelSelectScreen
{
    private Texture2D[] _levelButtons;
    private Rectangle[] _buttonRects;

    private const int TotalLevels = 14;

    public void LoadContent(ContentManager content)
    {
        _levelButtons = new Texture2D[TotalLevels];
        for (int i = 0; i < TotalLevels; i++)
            _levelButtons[i] = content.Load<Texture2D>($"Menu/Buttons/NumberedButtons/level_button_{i + 1}");

        _buttonRects = BuildButtonRects();
    }

    private static Rectangle[] BuildButtonRects()
    {
        const int cols = 5;
        const int btnSize = 140;
        const int spacing = 24;

        int rows = (int)Math.Ceiling(TotalLevels / (float)cols);
        int gridW = cols * btnSize + (cols - 1) * spacing;
        int gridH = rows * btnSize + (rows - 1) * spacing;
        int startX = (1920 - gridW) / 2;
        int startY = (1080 - gridH) / 2 + 60;

        var rects = new Rectangle[TotalLevels];
        for (int i = 0; i < TotalLevels; i++)
        {
            int col = i % cols;
            int row = i / cols;
            rects[i] = new Rectangle(
                startX + col * (btnSize + spacing),
                startY + row * (btnSize + spacing),
                btnSize,
                btnSize);
        }
        return rects;
    }

    // Returns 1-based level number when clicked, otherwise -1.
    public int Update(Point logicalMouse, bool mouseJustPressed)
    {
        if (!mouseJustPressed) return -1;

        for (int i = 0; i < _buttonRects.Length; i++)
        {
            if (_buttonRects[i].Contains(logicalMouse))
                return i + 1;
        }
        return -1;
    }

    public void Draw(SpriteBatch spriteBatch, Point logicalMouse)
    {
        for (int i = 0; i < TotalLevels; i++)
        {
            bool hovered = _buttonRects[i].Contains(logicalMouse);
            var tint = hovered ? new Color(255, 230, 130) : Color.White;
            spriteBatch.Draw(_levelButtons[i], _buttonRects[i], tint);
        }
    }
}
