using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct UpgradeOptionStruct
{
    public UpgradeType CardType;
    public int ID;             // ID of the weapon or passive
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
    public int CurrentLevel;   // Current level of this upgrade for the player
    public int MaxLevel;       // Max level (typically 5)

    public WeaponType WeaponType; // Valid only if CardType == Weapon
    public PassiveType PassiveType; // Valid only if CardType == Passive
}

public struct UpgradeEventArgs
{
    public UpgradeType upgradeType;
    public WeaponType weaponType;
    public PassiveType passiveType;
    public int id;
    public int level;
}