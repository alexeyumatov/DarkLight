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

    public static List<Tile> LoadTiles(ContentManager content, string levelAssetName, out Vector2 playerStart)
    {
        var charToTexture = BuildTextureMap(content);
        var levelText = File.ReadAllText(Path.Combine(content.RootDirectory, levelAssetName));
        var lines = levelText
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var tiles = new List<Tile>();
        playerStart = Vector2.Zero;

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

                if (!charToTexture.TryGetValue(symbol, out var texture))
                {
                    continue; // ignore non-tile symbols for now
                }

                tiles.Add(new Tile(texture, position));
            }
        }

        return tiles;
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
            ['\"'] = content.Load<Texture2D>("Locations/Platform"),
            ['#'] = content.Load<Texture2D>("Locations/1_PART"),
            ['№'] = content.Load<Texture2D>("Locations/2_PART"),
            ['!'] = content.Load<Texture2D>("Locations/3_PART"),
            ['&'] = content.Load<Texture2D>("Locations/4_PART")
        };
    }
}
