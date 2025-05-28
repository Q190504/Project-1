using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UpgradeOptionClass : MonoBehaviour
{
    public UpgradeType Type { get; set; }   // Type of upgrade (Weapon or Passive)
    public int ID { get; set; }             // ID of the weapon or passive
    public int CurrentLevel { get; set; }   // Current level of this upgrade for the player
    public int MaxLevel { get; }            // Max level (typically 5)


    public UpgradeOptionClass(UpgradeType type, int id)
    {
        Type = type;
        ID = id;
        CurrentLevel = 0;
        MaxLevel = 5;
    }

    public void LevelUp()
    {
        CurrentLevel++;
        if (CurrentLevel > MaxLevel)
        {
            CurrentLevel = MaxLevel; // Cap at max level
        }
    }
}

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