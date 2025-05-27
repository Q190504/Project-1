using Unity.Collections;
using Unity.Entities;

public struct UpgradeOption
{
    public UpgradeType Type;
    public int ID;             // ID of the weapon or passive
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
    public int CurrentLevel;   // Current level of this upgrade for the player
    public int MaxLevel;       // Max level (typically 5)
}