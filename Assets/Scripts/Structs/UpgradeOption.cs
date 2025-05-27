using Unity.Collections;
using Unity.Entities;

public struct UpgradeOption
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