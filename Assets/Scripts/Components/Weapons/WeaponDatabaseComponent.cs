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
    public int damage;
    public float cooldownTime;
    public float range;
}
