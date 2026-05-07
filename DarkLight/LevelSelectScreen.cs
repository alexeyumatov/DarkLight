using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class LevelSelectScreen
{
    private SpriteFont font;
    private Texture2D pixel;
    private Texture2D coinTex;
    private Texture2D[] levelButtons;
    private Rectangle[] buttonRects;
    private Rectangle marketBtnRect;

    private const int TotalLevels = 14;

    private static readonly Color MarketBtnNorm = new Color(55, 38, 95);
    private static readonly Color MarketBtnHov  = new Color(90, 65, 155);
    private static readonly Color BorderColor   = new Color(85, 65, 130);
    private static readonly Color GoldColor     = new Color(255, 215, 0);

    public void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        font    = content.Load<SpriteFont>("Font/Main_Font");
        coinTex = content.Load<Texture2D>("Objects/Coin/coin");

        pixel = new Texture2D(gd, 1, 1);
        pixel.SetData([Color.White]);

        levelButtons = new Texture2D[TotalLevels];
        for (var i = 0; i < TotalLevels; i++)
            levelButtons[i] = content.Load<Texture2D>($"Menu/Buttons/NumberedButtons/level_button_{i + 1}");

        buttonRects  = BuildButtonRects();
        marketBtnRect = new Rectangle(835, 890, 250, 65);
    }

    private static Rectangle[] BuildButtonRects()
    {
        const int cols    = 5;
        const int btnSize = 140;
        const int spacing = 24;

        var rows  = (int)Math.Ceiling(TotalLevels / (float)cols);
        var gridW = cols * btnSize + (cols - 1) * spacing;
        var gridH = rows * btnSize + (rows - 1) * spacing;
        var startX = (1920 - gridW) / 2;
        var startY = (1080 - gridH) / 2 + 40;

        var rects = new Rectangle[TotalLevels];
        for (var i = 0; i < TotalLevels; i++)
        {
            var col = i % cols;
            var row = i / cols;
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

        if (marketBtnRect.Contains(logicalMouse)) return 0;

        for (var i = 0; i < buttonRects.Length; i++)
            if (buttonRects[i].Contains(logicalMouse))
                return i + 1;

        return -1;
    }

    public void Draw(SpriteBatch spriteBatch, Point logicalMouse)
    {
        // Coin balance — centered at top
        var coinsText = $"  {PlayerData.Coins}";
        var ts = font.MeasureString(coinsText);
        var totalW = 40 + (int)ts.X;
        var coinIconX = 960 - totalW / 2;
        var coinIconY = 28;
        spriteBatch.Draw(coinTex, new Rectangle(coinIconX, coinIconY, 40, 60), Color.White);
        spriteBatch.DrawString(font, coinsText, new Vector2(coinIconX, coinIconY + 12), GoldColor);

        // Level buttons
        for (var i = 0; i < TotalLevels; i++)
        {
            var hov = buttonRects[i].Contains(logicalMouse);
            spriteBatch.Draw(levelButtons[i], buttonRects[i], hov ? new Color(255, 230, 130) : Color.White);
        }

        // Market button
        var marketHov = marketBtnRect.Contains(logicalMouse);
        spriteBatch.Draw(pixel, marketBtnRect, marketHov ? MarketBtnHov : MarketBtnNorm);
        Border(spriteBatch, marketBtnRect, BorderColor, 2);
        var shopText = "МАГАЗИН";
        var ss = font.MeasureString(shopText);
        spriteBatch.DrawString(font, shopText,
            new Vector2(marketBtnRect.X + (marketBtnRect.Width - ss.X) / 2,
                        marketBtnRect.Y + (marketBtnRect.Height - ss.Y) / 2), GoldColor);
    }

    private void Border(SpriteBatch sb, Rectangle r, Color c, int t)
    {
        sb.Draw(pixel, new Rectangle(r.X,         r.Y,          r.Width, t),    c);
        sb.Draw(pixel, new Rectangle(r.X,         r.Bottom - t, r.Width, t),    c);
        sb.Draw(pixel, new Rectangle(r.X,         r.Y,          t, r.Height),   c);
        sb.Draw(pixel, new Rectangle(r.Right - t, r.Y,          t, r.Height),   c);
    }
}
