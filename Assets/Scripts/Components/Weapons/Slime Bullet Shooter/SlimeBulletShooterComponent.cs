using Unity.Entities;

public struct SlimeBulletShooterComponent : IComponentData
{
    public float delay;         // delay between bullets
    public float timer;
    public int damage;
    public float cooldown;
    public int bulletCount;
    public int bulletsRemaining;
    public float minimumDistance;
    public float minimumDistanceBetweenBullets;
    public float maximumDistanceBetweenBullets;
    public float previousDistance;
    public float passthroughDamageModifier;
    public float moveSpeed;
    public float existDuration;
    public bool isSlimeFrenzyActive;
    public float bonusDamagePercent;
    public float slowModifier;
    public float slowRadius;
}
