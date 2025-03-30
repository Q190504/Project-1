using Unity.Entities;
using Unity.Collections;

public enum WeaponType { SlimeSlash }

public struct WeaponStatsLevel
{
    public float damage;
    public float range;
    public float cooldownTime;
}

public struct WeaponStatsBlob
{
    public WeaponType Type;
    public BlobArray<WeaponStatsLevel> Levels;
}