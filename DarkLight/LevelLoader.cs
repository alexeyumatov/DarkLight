using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public static class LevelLoader
{
    public const int TileSize = 128;

    public static List<Tile> LoadTiles(ContentManager content, string levelAssetName,
                                       out Vector2 playerStart, out List<Coin> coins,
                                       out List<Enemy> enemies)
    {
        var coinTexture = content.Load<Texture2D>("Objects/Coin/coin");
        var charToTexture = BuildTextureMap(content);
        var levelText = File.ReadAllText(Path.Combine(content.RootDirectory, levelAssetName));
        var lines = levelText
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var tiles = new List<Tile>();
        coins = new List<Coin>();
        enemies = new List<Enemy>();
        playerStart = Vector2.Zero;

        var weakIdle    = LoadFrames(content, "Enemies/RegularEnemy/{0}",              1, 8);
        var weakDeath   = LoadFrames(content, "Enemies/RegularEnemy/EnemyDeath/{0}",   1, 12);
        var midIdle     = LoadFrames(content, "Enemies/MiddleEnemy/Enemy_2_Idle_{0}",  2, 9);
        var midDeath    = LoadFrames(content, "Enemies/MiddleEnemy/EnemyDeath/{0}",    1, 12);
        var midBullets  = LoadFrames(content, "Enemies/MiddleEnemy/Bullets/enemy_bullet_{0}", 1, 6);
        var hardIdle    = LoadFrames(content, "Enemies/HardEnemy/ENEMY_3_IDLE_{0}",    1, 12);
        var hardDeath   = LoadFrames(content, "Enemies/HardEnemy/EnemyDeath/{0}",      1, 12);
        var hardBullets = LoadFrames(content, "Enemies/HardEnemy/Bullets/ENEMY_3_BULLET_{0}", 1, 6);

        for (var y = 0; y < lines.Length; y++)
        {
            var row = lines[y];
            for (var x = 0; x < row.Length; x++)
            {
                var symbol = row[x];
                var position = new Vector2(x * TileSize, y * TileSize);

                if (symbol == 'P')
                {
                    playerStart = position;
                    continue;
                }

                if (symbol == 't')
                {
                    coins.Add(new Coin(coinTexture, position));
                    continue;
                }

                if (symbol == 'E')
                {
                    enemies.Add(new Enemy(EnemyType.Weak, position, weakIdle, weakDeath, System.Array.Empty<Texture2D>(), 160, 270));
                    continue;
                }

                if (symbol == 'M')
                {
                    enemies.Add(new Enemy(EnemyType.Middle, position, midIdle, midDeath, midBullets, 180, 300));
                    continue;
                }

                if (symbol == 'H')
                {
                    enemies.Add(new Enemy(EnemyType.Strong, position, hardIdle, hardDeath, hardBullets, 152, 263));
                    continue;
                }

                if (!charToTexture.TryGetValue(symbol, out var texture))
                    continue;

                bool isLadder = symbol == '|';
                bool isPortal = symbol is '#' or '№' or '!' or '&';
                bool isCollidable = !isLadder && !isPortal;
                tiles.Add(new Tile(texture, position, isCollidable, isLadder, isPortal));
            }
        }

        return tiles;
    }

    private static Texture2D[] LoadFrames(ContentManager content, string pattern, int from, int to)
    {
        var frames = new Texture2D[to - from + 1];
        for (int i = from; i <= to; i++)
            frames[i - from] = content.Load<Texture2D>(string.Format(pattern, i));
        return frames;
    }

    private static Dictionary<char, Texture2D> BuildTextureMap(ContentManager content)
    {
        return new Dictionary<char, Texture2D>
        {
            ['{'] = content.Load<Texture2D>("Locations/Top_Left_Corner"),
            ['-'] = content.Load<Texture2D>("Locations/Roof"),
            ['}'] = content.Load<Texture2D>("Locations/Top_Right_Corner"),
            ['$'] = content.Load<Texture2D>("Locations/Left_Wall"),
            ['['] = content.Load<Texture2D>("Locations/Bottom_Left_Corner"),
            ['_'] = content.Load<Texture2D>("Locations/Floor"),
            [']'] = content.Load<Texture2D>("Locations/Bottom_Right_Corner"),
            ['/'] = content.Load<Texture2D>("Locations/Right_Wall"),
            ['|'] = content.Load<Texture2D>("Locations/Ladder"),
            ['+'] = content.Load<Texture2D>("Locations/Dark_Block"),
            [':'] = content.Load<Texture2D>("Locations/Left_Platform_Corner"),
            [';'] = content.Load<Texture2D>("Locations/Right_Platform_Corner"),
            ['"'] = content.Load<Texture2D>("Locations/Platform"),
            ['#'] = content.Load<Texture2D>("Locations/1_PART"),
            ['№'] = content.Load<Texture2D>("Locations/2_PART"),
            ['!'] = content.Load<Texture2D>("Locations/3_PART"),
            ['&'] = content.Load<Texture2D>("Locations/4_PART")
        };
    }
}
