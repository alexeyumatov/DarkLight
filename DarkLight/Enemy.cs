using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DarkLight;

public enum EnemyType { Weak, Middle, Strong }

public class Enemy
{
    public Vector2 Position;
    public Vector2 Velocity;

    private readonly EnemyType type;
    private int hp;
    private bool isDying;
    private bool isFullyDead;

    private readonly Texture2D[] idleFrames;
    private readonly Texture2D[] deathFrames;
    private readonly Texture2D[] bulletFrames;

    private int frameIndex;
    private float frameTimer;
    private float shootTimer;
    private bool facingRight = true;

    public bool IsFullyDead => isFullyDead;
    public bool IsAlive => !isDying && !isFullyDead;
    public EnemyType Type => type;
    public int ContactDamage => 15;

    private readonly int renderW;
    private readonly int renderH;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, renderW, renderH);

    private int MaxHp => type switch { EnemyType.Weak => 40, EnemyType.Middle => 80, _ => 150 };
    private float MoveSpeed => type switch { EnemyType.Weak => 120f, EnemyType.Middle => 80f, _ => 60f };
    private float PreferredDist => type switch { EnemyType.Middle => 400f, _ => 500f };
    private int BulletDamage => type switch { EnemyType.Middle => 8, _ => 20 };
    private float ShootInterval => type switch { EnemyType.Middle => 2.5f, _ => 2.0f };

    private const float Gravity = 3500f;
    private const float IdleFrameSec = 0.12f;
    private const float DeathFrameSec = 0.08f;

    public Enemy(EnemyType type, Vector2 position,
                 Texture2D[] idleFrames, Texture2D[] deathFrames,
                 Texture2D[] bulletFrames, int renderW, int renderH)
    {
        this.type = type;
        Position = position;
        this.idleFrames = idleFrames;
        this.deathFrames = deathFrames;
        this.bulletFrames = bulletFrames;
        this.renderW = renderW;
        this.renderH = renderH;
        hp = MaxHp;
    }

    public void TakeDamage(int amount)
    {
        if (isDying || isFullyDead) return;
        hp -= amount;
        if (hp <= 0)
        {
            isDying = true;
            frameIndex = 0;
            frameTimer = 0f;
        }
    }

    public EnemyBullet Update(GameTime gameTime, Player player, List<Tile> tiles)
    {
        if (isFullyDead) return null;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var frames = isDying ? deathFrames : idleFrames;
        float frameDur = isDying ? DeathFrameSec : IdleFrameSec;

        frameTimer += dt;
        if (frameTimer >= frameDur)
        {
            frameTimer -= frameDur;
            if (isDying)
            {
                if (frameIndex < frames.Length - 1) frameIndex++;
                else { isFullyDead = true; return null; }
            }
            else
            {
                frameIndex = (frameIndex + 1) % frames.Length;
            }
        }

        if (isDying) return null;

        float dx = player.Position.X - Position.X;
        facingRight = dx > 0;

        EnemyBullet spawned = null;

        if (type == EnemyType.Weak)
        {
            Velocity.X = facingRight ? MoveSpeed : -MoveSpeed;
        }
        else
        {
            float dist = MathF.Abs(dx);
            if (dist > PreferredDist + 60f)
                Velocity.X = facingRight ? MoveSpeed : -MoveSpeed;
            else if (dist < PreferredDist - 60f)
                Velocity.X = facingRight ? -MoveSpeed : MoveSpeed;
            else
                Velocity.X = 0f;

            shootTimer -= dt;
            if (shootTimer <= 0f && bulletFrames.Length > 0)
            {
                shootTimer = ShootInterval;
                bool poison = type == EnemyType.Strong;
                float bulletX = facingRight ? Position.X + renderW : Position.X - 30;
                float bulletY = Position.Y + renderH / 2f - 15;
                spawned = new EnemyBullet(bulletFrames,
                    new Vector2(bulletX, bulletY), facingRight, BulletDamage, poison);
            }
        }

        Velocity.Y += Gravity * dt;

        Position.X += Velocity.X * dt;
        HandleCollisions(tiles, horizontal: true);

        Position.Y += Velocity.Y * dt;
        HandleCollisions(tiles, horizontal: false);

        return spawned;
    }

    private void HandleCollisions(List<Tile> tiles, bool horizontal)
    {
        var bounds = Bounds;
        foreach (var tile in tiles.Where(t => t.IsCollidable && bounds.Intersects(t.Bounds)))
        {
            if (horizontal)
            {
                Position.X = Velocity.X switch
                {
                    > 0 => tile.Bounds.Left - renderW,
                    < 0 => tile.Bounds.Right,
                    _ => Position.X
                };
                Velocity.X = 0f;
            }
            else
            {
                switch (Velocity.Y)
                {
                    case > 0:
                        Position.Y = tile.Bounds.Top - renderH;
                        break;
                    case < 0:
                        Position.Y = tile.Bounds.Bottom;
                        break;
                }
                Velocity.Y = 0f;
            }
            bounds = Bounds;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (isFullyDead) return;
        var frames = isDying ? deathFrames : idleFrames;
        var tex = frames[Math.Min(frameIndex, frames.Length - 1)];
        var effect = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(tex,
            new Rectangle((int)Position.X, (int)Position.Y, renderW, renderH),
            null, Color.White, 0f, Vector2.Zero, effect, 0f);
    }
}
