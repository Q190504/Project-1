using Unity.Entities;

public struct SlimeBulletShooterComponent : IComponentData
{
    public float delay;         // delay between bullets
    public float timer;
    //public EntityCommandBuffer ecb;
    public int damage;
    public float cooldown;
    public int bulletCount;
    public int bulletsRemaining;
    public float minimumDistance;
    public float minimumDistanceBetweenBullets;
    public float maximumDistanceBetweenBullets;
    public float previousDistance;
    public float passthroughDamageModifier;
    public int previousDamage;
    public float moveSpeed;
    public float existDuration;
    public bool isSlimeFrenzyActive;
    public float bonusDamagePercent;
}
