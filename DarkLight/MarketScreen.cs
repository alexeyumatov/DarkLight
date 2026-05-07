using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class MarketScreen
{
    private SpriteFont _font;
    private Texture2D _pixel;
    private Texture2D _coinTex;

    // Upgrade icons (270×270 → displayed at 130×130)
    private Texture2D _shieldIcon, _cooldownIcon, _damageIcon;

    // Buy button sprites per upgrade: [0]=Available, [1]=NotEnoughCoins, [2]=Maximum (270×60)
    private Texture2D[] _shieldBtn   = new Texture2D[3];
    private Texture2D[] _cooldownBtn = new Texture2D[3];
    private Texture2D[] _damageBtn   = new Texture2D[3];

    // Clickable hitboxes (set in LoadContent)
    private Rectangle _shieldBtnRect;
    private Rectangle _cooldownBtnRect;
    private Rectangle _damageBtnRect;
    private Rectangle _backBtnRect;

    // Layout constants (logical 1920×1080)
    private const int PanelX  = 240;
    private const int PanelW  = 1440;
    private const int RowH    = 190;
    private const int Row1Y   = 230;
    private const int Row2Y   = 450;
    private const int Row3Y   = 670;
    private const int IconX   = 268;
    private const int IconSz  = 130;
    private const int TextX   = 430;
    private const int BtnX    = 1350;
    private const int BtnW    = 270;
    private const int BtnH    = 60;

    private static readonly Color PanelColor  = new Color(35, 25, 60);
    private static readonly Color BorderColor = new Color(85, 65, 130);
    private static readonly Color GoldColor   = new Color(255, 215, 0);
    private static readonly Color BackBtnNorm = new Color(55, 38, 95);
    private static readonly Color BackBtnHov  = new Color(90, 65, 155);

    public void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        _font    = content.Load<SpriteFont>("Font/Main_Font");
        _coinTex = content.Load<Texture2D>("Objects/Coin/coin");

        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _shieldIcon   = content.Load<Texture2D>("Menu/Market/armor_upgrade");
        _cooldownIcon = content.Load<Texture2D>("Menu/Market/bullet_cooldown_upgrade");
        _damageIcon   = content.Load<Texture2D>("Menu/Market/bullet_damage_upgrade");

        var maxSprite = content.Load<Texture2D>("Menu/Market/MarketButtons/Maximum/max_amount");

        _shieldBtn[0]   = content.Load<Texture2D>("Menu/Market/MarketButtons/Available/shield_points");
        _shieldBtn[1]   = content.Load<Texture2D>("Menu/Market/MarketButtons/NotEnoughCoins/shield_points");
        _shieldBtn[2]   = maxSprite;

        _cooldownBtn[0] = content.Load<Texture2D>("Menu/Market/MarketButtons/Available/bullets_collide");
        _cooldownBtn[1] = content.Load<Texture2D>("Menu/Market/MarketButtons/NotEnoughCoins/bullets_collide");
        _cooldownBtn[2] = maxSprite;

        _damageBtn[0]   = content.Load<Texture2D>("Menu/Market/MarketButtons/Available/bullets_damage");
        _damageBtn[1]   = content.Load<Texture2D>("Menu/Market/MarketButtons/NotEnoughCoins/bullets_damage");
        _damageBtn[2]   = maxSprite;

        _shieldBtnRect   = BtnRect(Row1Y);
        _cooldownBtnRect = BtnRect(Row2Y);
        _damageBtnRect   = BtnRect(Row3Y);
        _backBtnRect     = new Rectangle(60, 960, 230, 65);
    }

    private static Rectangle BtnRect(int rowY) =>
        new(BtnX, rowY + (RowH - BtnH) / 2, BtnW, BtnH);

    private enum BtnState { Available, NotEnoughCoins, Maximum }

    private static BtnState State(int level, int maxLevel, int cost) =>
        level >= maxLevel       ? BtnState.Maximum :
        PlayerData.Coins >= cost ? BtnState.Available :
                                   BtnState.NotEnoughCoins;

    // Returns true when the user wants to go back.
    public bool Update(Point logicalMouse, bool mouseJustPressed)
    {
        if (!mouseJustPressed) return false;

        if (_backBtnRect.Contains(logicalMouse)) return true;

        if (_shieldBtnRect.Contains(logicalMouse) &&
            State(PlayerData.ShieldLevel, PlayerData.MaxShieldLevel, PlayerData.ShieldUpgradeCost) == BtnState.Available)
        {
            PlayerData.Coins      -= PlayerData.ShieldUpgradeCost;
            PlayerData.ShieldLevel++;
        }

        if (_cooldownBtnRect.Contains(logicalMouse) &&
            State(PlayerData.CooldownLevel, PlayerData.MaxCooldownLevel, PlayerData.CooldownUpgradeCost) == BtnState.Available)
        {
            PlayerData.Coins        -= PlayerData.CooldownUpgradeCost;
            PlayerData.CooldownLevel++;
        }

        if (_damageBtnRect.Contains(logicalMouse) &&
            State(PlayerData.DamageLevel, PlayerData.MaxDamageLevel, PlayerData.DamageUpgradeCost) == BtnState.Available)
        {
            PlayerData.Coins      -= PlayerData.DamageUpgradeCost;
            PlayerData.DamageLevel++;
        }

        return false;
    }

    public void Draw(SpriteBatch spriteBatch, Point logicalMouse)
    {
        // Title
        string title = "МАГАЗИН";
        var ts = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2(960 - ts.X / 2, 60), GoldColor);

        // Coin balance (top right)
        spriteBatch.Draw(_coinTex, new Rectangle(1660, 52, 40, 60), Color.White);
        spriteBatch.DrawString(_font, $"{PlayerData.Coins}", new Vector2(1708, 64), GoldColor);

        // Upgrade rows
        DrawRow(spriteBatch, logicalMouse, Row1Y, _shieldIcon, _shieldBtn, _shieldBtnRect,
            "ЩИТ",
            $"Уровень: {PlayerData.ShieldLevel}/{PlayerData.MaxShieldLevel}   {PlayerData.ShieldValue}/50",
            $"Цена: {PlayerData.ShieldUpgradeCost} монет",
            PlayerData.ShieldLevel, PlayerData.MaxShieldLevel, PlayerData.ShieldUpgradeCost);

        int curMs  = 100 - PlayerData.CooldownLevel * 10;
        int nextMs = PlayerData.CooldownLevel < PlayerData.MaxCooldownLevel ? curMs - 10 : curMs;
        string cdStats = PlayerData.CooldownLevel < PlayerData.MaxCooldownLevel
            ? $"Уровень: {PlayerData.CooldownLevel}/{PlayerData.MaxCooldownLevel}   {curMs} мс -> {nextMs} мс"
            : $"Уровень: {PlayerData.CooldownLevel}/{PlayerData.MaxCooldownLevel}   {curMs} мс";

        DrawRow(spriteBatch, logicalMouse, Row2Y, _cooldownIcon, _cooldownBtn, _cooldownBtnRect,
            "СКОРОСТРЕЛЬНОСТЬ",
            cdStats,
            $"Цена: {PlayerData.CooldownUpgradeCost} монет",
            PlayerData.CooldownLevel, PlayerData.MaxCooldownLevel, PlayerData.CooldownUpgradeCost);

        int curDmg  = PlayerData.BulletDamage;
        int nextDmg = PlayerData.DamageLevel < PlayerData.MaxDamageLevel ? curDmg + 5 : curDmg;
        string dmgStats = PlayerData.DamageLevel < PlayerData.MaxDamageLevel
            ? $"Уровень: {PlayerData.DamageLevel}/{PlayerData.MaxDamageLevel}   {curDmg} -> {nextDmg}"
            : $"Уровень: {PlayerData.DamageLevel}/{PlayerData.MaxDamageLevel}   {curDmg}";

        DrawRow(spriteBatch, logicalMouse, Row3Y, _damageIcon, _damageBtn, _damageBtnRect,
            "УРОН ПУЛИ",
            dmgStats,
            $"Цена: {PlayerData.DamageUpgradeCost} монет",
            PlayerData.DamageLevel, PlayerData.MaxDamageLevel, PlayerData.DamageUpgradeCost);

        // Back button
        bool backHov = _backBtnRect.Contains(logicalMouse);
        FillRect(spriteBatch, _backBtnRect, backHov ? BackBtnHov : BackBtnNorm);
        Border(spriteBatch, _backBtnRect, BorderColor, 2);
        string backText = "< НАЗАД";
        var bs = _font.MeasureString(backText);
        spriteBatch.DrawString(_font, backText,
            new Vector2(_backBtnRect.X + (_backBtnRect.Width - bs.X) / 2,
                        _backBtnRect.Y + (_backBtnRect.Height - bs.Y) / 2), Color.White);
    }

    private void DrawRow(SpriteBatch sb, Point mouse, int rowY,
                         Texture2D icon, Texture2D[] btnSprites, Rectangle btnRect,
                         string name, string stats, string priceText,
                         int level, int maxLevel, int cost)
    {
        var panel = new Rectangle(PanelX, rowY, PanelW, RowH);
        FillRect(sb, panel, PanelColor);
        Border(sb, panel, BorderColor, 2);

        // Icon
        sb.Draw(icon, new Rectangle(IconX, rowY + (RowH - IconSz) / 2, IconSz, IconSz), Color.White);

        // Text
        int ty = rowY + 30;
        sb.DrawString(_font, name, new Vector2(TextX, ty), GoldColor);
        sb.DrawString(_font, stats, new Vector2(TextX, ty + 44), Color.White);

        var state = State(level, maxLevel, cost);
        if (state != BtnState.Maximum)
        {
            var priceColor = state == BtnState.Available ? Color.LightGreen : Color.OrangeRed;
            sb.DrawString(_font, priceText, new Vector2(TextX, ty + 90), priceColor);
        }

        // Button sprite
        bool hov = state == BtnState.Available && btnRect.Contains(mouse);
        var tint = hov ? new Color(255, 230, 130) : Color.White;
        sb.Draw(btnSprites[(int)state], btnRect, tint);
    }

    private void FillRect(SpriteBatch sb, Rectangle r, Color c) =>
        sb.Draw(_pixel, r, c);

    private void Border(SpriteBatch sb, Rectangle r, Color c, int t)
    {
        sb.Draw(_pixel, new Rectangle(r.X,          r.Y,          r.Width, t),     c);
        sb.Draw(_pixel, new Rectangle(r.X,          r.Bottom - t, r.Width, t),     c);
        sb.Draw(_pixel, new Rectangle(r.X,          r.Y,          t, r.Height),    c);
        sb.Draw(_pixel, new Rectangle(r.Right - t,  r.Y,          t, r.Height),    c);
    }
}
