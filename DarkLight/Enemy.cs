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

    private readonly EnemyType _type;
    private int _hp;
    private bool _isDying;
    private bool _isFullyDead;

    private readonly Texture2D[] _idleFrames;
    private readonly Texture2D[] _deathFrames;
    private readonly Texture2D[] _bulletFrames;

    private int _frameIndex;
    private float _frameTimer;
    private float _shootTimer;
    private bool _facingRight = true;

    public bool IsFullyDead => _isFullyDead;
    public bool IsAlive => !_isDying && !_isFullyDead;
    public EnemyType Type => _type;
    public int ContactDamage => 15;

    private readonly int _renderW;
    private readonly int _renderH;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, _renderW, _renderH);

    private int MaxHp => _type switch { EnemyType.Weak => 40, EnemyType.Middle => 80, _ => 150 };
    private float MoveSpeed => _type switch { EnemyType.Weak => 120f, EnemyType.Middle => 80f, _ => 60f };
    private float PreferredDist => _type switch { EnemyType.Middle => 400f, _ => 500f };
    private int BulletDamage => _type switch { EnemyType.Middle => 8, _ => 20 };
    private float ShootInterval => _type switch { EnemyType.Middle => 2.5f, _ => 2.0f };

    private const float Gravity = 3500f;
    private const float IdleFrameSec = 0.12f;
    private const float DeathFrameSec = 0.08f;

    public Enemy(EnemyType type, Vector2 position,
                 Texture2D[] idleFrames, Texture2D[] deathFrames,
                 Texture2D[] bulletFrames, int renderW, int renderH)
    {
        _type = type;
        Position = position;
        _idleFrames = idleFrames;
        _deathFrames = deathFrames;
        _bulletFrames = bulletFrames;
        _renderW = renderW;
        _renderH = renderH;
        _hp = MaxHp;
    }

    public void TakeDamage(int amount)
    {
        if (_isDying || _isFullyDead) return;
        _hp -= amount;
        if (_hp <= 0)
        {
            _isDying = true;
            _frameIndex = 0;
            _frameTimer = 0f;
        }
    }

    public EnemyBullet Update(GameTime gameTime, Player player, List<Tile> tiles)
    {
        if (_isFullyDead) return null;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var frames = _isDying ? _deathFrames : _idleFrames;
        float frameDur = _isDying ? DeathFrameSec : IdleFrameSec;

        _frameTimer += dt;
        if (_frameTimer >= frameDur)
        {
            _frameTimer -= frameDur;
            if (_isDying)
            {
                if (_frameIndex < frames.Length - 1) _frameIndex++;
                else { _isFullyDead = true; return null; }
            }
            else
            {
                _frameIndex = (_frameIndex + 1) % frames.Length;
            }
        }

        if (_isDying) return null;

        float dx = player.Position.X - Position.X;
        _facingRight = dx > 0;

        EnemyBullet spawned = null;

        if (_type == EnemyType.Weak)
        {
            Velocity.X = _facingRight ? MoveSpeed : -MoveSpeed;
        }
        else
        {
            float dist = MathF.Abs(dx);
            if (dist > PreferredDist + 60f)
                Velocity.X = _facingRight ? MoveSpeed : -MoveSpeed;
            else if (dist < PreferredDist - 60f)
                Velocity.X = _facingRight ? -MoveSpeed : MoveSpeed;
            else
                Velocity.X = 0f;

            _shootTimer -= dt;
            if (_shootTimer <= 0f && _bulletFrames.Length > 0)
            {
                _shootTimer = ShootInterval;
                bool poison = _type == EnemyType.Strong;
                float bulletX = _facingRight ? Position.X + _renderW : Position.X - 30;
                float bulletY = Position.Y + _renderH / 2f - 15;
                spawned = new EnemyBullet(_bulletFrames,
                    new Vector2(bulletX, bulletY), _facingRight, BulletDamage, poison);
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
                    > 0 => tile.Bounds.Left - _renderW,
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
                        Position.Y = tile.Bounds.Top - _renderH;
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
        if (_isFullyDead) return;
        var frames = _isDying ? _deathFrames : _idleFrames;
        var tex = frames[Math.Min(_frameIndex, frames.Length - 1)];
        var effect = _facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(tex,
            new Rectangle((int)Position.X, (int)Position.Y, _renderW, _renderH),
            null, Color.White, 0f, Vector2.Zero, effect, 0f);
    }
}
