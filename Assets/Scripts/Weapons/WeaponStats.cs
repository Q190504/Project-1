using System;
using UnityEngine;

public enum WeaponType { SlimeBullet, SlimeSlash }

[Serializable]
public class WeaponLevelData
{
    [Header("Basic")]
    public int damage;
    public float cooldownTime;

    //SlimeBullet
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

[Serializable]
public class WeaponStats
{
    public WeaponType type;
    public WeaponLevelData[] levels = new WeaponLevelData[6];
}