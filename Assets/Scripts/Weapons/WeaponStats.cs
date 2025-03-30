using System;
using Unity.Entities;

public enum WeaponType { SlimeSlash }

[Serializable]
public class WeaponLevelData
{
    public int damage;
    public float range;
    public float cooldownTime;
}

[Serializable]
public class WeaponStats
{
    public WeaponType type;
    public WeaponLevelData[] levels = new WeaponLevelData[6];
}