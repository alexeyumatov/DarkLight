namespace DarkLight;

public static class PlayerData
{
    public static int Coins { get; set; }

    // ── Shield ──────────────────────────────────────────────────────────────
    public static int ShieldLevel { get; set; }
    public const int MaxShieldLevel    = 5;
    public const int ShieldUpgradeCost = 10;
    public static int ShieldValue => ShieldLevel * 10; // 0 → 10 → … → 50

    // ── Bullet cooldown ─────────────────────────────────────────────────────
    public static int CooldownLevel { get; set; }
    public const int MaxCooldownLevel    = 4;
    public const int CooldownUpgradeCost = 5;
    public static float BulletCooldown => (100 - CooldownLevel * 10) / 1000f; // 0.1s → 0.06s

    // ── Bullet damage ────────────────────────────────────────────────────────
    public static int DamageLevel { get; set; }
    public const int MaxDamageLevel    = 3;
    public const int DamageUpgradeCost = 15;
    public static int BulletDamage => 10 + DamageLevel * 5; // 10 → 15 → 20 → 25
}
