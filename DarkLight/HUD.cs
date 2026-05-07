using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public class HUD
{
    private SpriteFont font;
    private Texture2D heart;
    private Texture2D poisonedHeart;
    private Texture2D shield;
    private Texture2D dashReady;
    private Texture2D ultimateReady;
    private Texture2D ultimateNotReady;

    public void LoadContent(ContentManager content)
    {
        font = content.Load<SpriteFont>("Font/Main_Font");
        
        heart = content.Load<Texture2D>("PlayerData/heart");
        poisonedHeart = content.Load<Texture2D>("PlayerData/poisoned_heart");
        shield = content.Load<Texture2D>("PlayerData/shield");
        dashReady = content.Load<Texture2D>("PlayerData/Dash/dash_is_ready");
        ultimateReady = content.Load<Texture2D>("PlayerData/UltimateAttack/ultimate_ready");
        ultimateNotReady = content.Load<Texture2D>("PlayerData/UltimateAttack/ultimate_not_ready");
    }

    public void Draw(SpriteBatch spriteBatch, Player hero, bool ultimateAttack, bool heroIsPoisoned, bool dashIsReady)
    {
        var hpData = hero.HealthPoints.ToString();
        var shieldData = hero.ShieldPoints.ToString();
        
        var iconSize = new Point(74, 64);
        var ultimateIconSize = new Point(90, 90);
        
        if (dashIsReady)
        {
            spriteBatch.Draw(dashReady, new Rectangle(1400, 965, iconSize.X, iconSize.Y), Color.White);
        }

        string staminaData;
        
        switch (hero.Stamina)
        {
            case 100 when ultimateAttack:
                spriteBatch.Draw(ultimateReady, new Rectangle(900, 955, ultimateIconSize.X, ultimateIconSize.Y), Color.White);
                staminaData = "In Use";
                break;
            case 100:
                spriteBatch.Draw(ultimateReady, new Rectangle(900, 955, ultimateIconSize.X, ultimateIconSize.Y), Color.White);
                staminaData = "Ready";
                break;
            default:
                spriteBatch.Draw(ultimateNotReady, new Rectangle(900, 955, ultimateIconSize.X, ultimateIconSize.Y), Color.White);
                staminaData = hero.Stamina.ToString();
                break;
        }
        
        spriteBatch.DrawString(font, hpData, new Vector2(690, 987), Color.White);

        spriteBatch.Draw(heroIsPoisoned ? poisonedHeart : heart, new Rectangle(600, 970, iconSize.X, iconSize.Y), Color.White);

        spriteBatch.DrawString(font, shieldData, new Vector2(1290, 987), Color.White);
        spriteBatch.Draw(shield, new Rectangle(1200, 970, iconSize.X, iconSize.Y), Color.White);
        
        spriteBatch.DrawString(font, staminaData, new Vector2(1006, 987), Color.White);
    }
}
