using Unity.Collections;
using Unity.Entities;

// Player Level Up Event
public struct PlayerLevelUpEvent : IComponentData { }

// Weapon/Passive Level Up Events
public struct UpgradeEvent : IComponentData { }

public struct WeaponComponent : IComponentData
{
    public WeaponType WeaponType;
    public int ID;
    public int Level;
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
}

public struct PassiveComponent : IComponentData
{
    public PassiveType PassiveType;
    public int ID;
    public int Level;
    public int MaxLevel;
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
}