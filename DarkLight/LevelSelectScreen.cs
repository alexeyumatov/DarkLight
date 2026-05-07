using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class LevelSelectScreen
{
    private SpriteFont _font;
    private Texture2D _pixel;
    private Texture2D _coinTex;
    private Texture2D[] _levelButtons;
    private Rectangle[] _buttonRects;
    private Rectangle _marketBtnRect;

    private const int TotalLevels = 14;

    private static readonly Color MarketBtnNorm = new Color(55, 38, 95);
    private static readonly Color MarketBtnHov  = new Color(90, 65, 155);
    private static readonly Color BorderColor   = new Color(85, 65, 130);
    private static readonly Color GoldColor     = new Color(255, 215, 0);

    public void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        _font    = content.Load<SpriteFont>("Font/Main_Font");
        _coinTex = content.Load<Texture2D>("Objects/Coin/coin");

        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _levelButtons = new Texture2D[TotalLevels];
        for (int i = 0; i < TotalLevels; i++)
            _levelButtons[i] = content.Load<Texture2D>($"Menu/Buttons/NumberedButtons/level_button_{i + 1}");

        _buttonRects  = BuildButtonRects();
        _marketBtnRect = new Rectangle(835, 890, 250, 65);
    }

    private static Rectangle[] BuildButtonRects()
    {
        const int cols    = 5;
        const int btnSize = 140;
        const int spacing = 24;

        int rows  = (int)Math.Ceiling(TotalLevels / (float)cols);
        int gridW = cols * btnSize + (cols - 1) * spacing;
        int gridH = rows * btnSize + (rows - 1) * spacing;
        int startX = (1920 - gridW) / 2;
        int startY = (1080 - gridH) / 2 + 40;

        var rects = new Rectangle[TotalLevels];
        for (int i = 0; i < TotalLevels; i++)
        {
            int col = i % cols;
            int row = i / cols;
            rects[i] = new Rectangle(
                startX + col * (btnSize + spacing),
                startY + row * (btnSize + spacing),
                btnSize, btnSize);
        }
        return rects;
    }

    // Returns: >0 = level number, 0 = market button, -1 = nothing clicked.
    public int Update(Point logicalMouse, bool mouseJustPressed)
    {
        if (!mouseJustPressed) return -1;

        if (_marketBtnRect.Contains(logicalMouse)) return 0;

        for (int i = 0; i < _buttonRects.Length; i++)
            if (_buttonRects[i].Contains(logicalMouse))
                return i + 1;

        return -1;
    }

    public void Draw(SpriteBatch spriteBatch, Point logicalMouse)
    {
        // Coin balance — centered at top
        string coinsText = $"  {PlayerData.Coins}";
        var ts = _font.MeasureString(coinsText);
        int totalW = 40 + (int)ts.X;
        int coinIconX = 960 - totalW / 2;
        int coinIconY = 28;
        spriteBatch.Draw(_coinTex, new Rectangle(coinIconX, coinIconY, 40, 60), Color.White);
        spriteBatch.DrawString(_font, coinsText, new Vector2(coinIconX, coinIconY + 12), GoldColor);

        // Level buttons
        for (int i = 0; i < TotalLevels; i++)
        {
            bool hov = _buttonRects[i].Contains(logicalMouse);
            spriteBatch.Draw(_levelButtons[i], _buttonRects[i], hov ? new Color(255, 230, 130) : Color.White);
        }

        // Market button
        bool marketHov = _marketBtnRect.Contains(logicalMouse);
        spriteBatch.Draw(_pixel, _marketBtnRect, marketHov ? MarketBtnHov : MarketBtnNorm);
        Border(spriteBatch, _marketBtnRect, BorderColor, 2);
        string shopText = "МАГАЗИН";
        var ss = _font.MeasureString(shopText);
        spriteBatch.DrawString(_font, shopText,
            new Vector2(_marketBtnRect.X + (_marketBtnRect.Width - ss.X) / 2,
                        _marketBtnRect.Y + (_marketBtnRect.Height - ss.Y) / 2), GoldColor);
    }

    private void Border(SpriteBatch sb, Rectangle r, Color c, int t)
    {
        sb.Draw(_pixel, new Rectangle(r.X,         r.Y,          r.Width, t),    c);
        sb.Draw(_pixel, new Rectangle(r.X,         r.Bottom - t, r.Width, t),    c);
        sb.Draw(_pixel, new Rectangle(r.X,         r.Y,          t, r.Height),   c);
        sb.Draw(_pixel, new Rectangle(r.Right - t, r.Y,          t, r.Height),   c);
    }
}
