using Unity.Entities;
using UnityEngine;

public struct WeaponDatabaseComponent : IComponentData
{
    public BlobAssetReference<WeaponDatabase> weaponDatabase;
}

public struct WeaponDatabase
{
    public BlobArray<WeaponStatsBlob> weapons;
}

public struct WeaponStatsBlob
{
    public WeaponType type;
    public BlobArray<WeaponStatsLevel> levels;
}

public struct WeaponStatsLevel
{
    [Header("Basic")]
    public int damage;
    public float cooldownTime;

    [Header("Slime Bullet")]
    public int bulletCount;
    public float maximumDistanceBetweenBullets;
    public float minimumDistanceBetweenBullets;
    public float passthroughDamageModifier;
    public float moveSpeed;
    public float distance;
    public float existDuration;
    public float slowModifier;
    public float slowRadius;

    [Header("Slime Slash")]
    public float radius;
}
