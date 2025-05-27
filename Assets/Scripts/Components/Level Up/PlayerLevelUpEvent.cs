using Unity.Collections;
using Unity.Entities;

// Player Level Up Event
public struct PlayerLevelUpEvent : IComponentData { }

// Weapon Level Up Events
public struct SlimeBulletShooterLevelUpEvent : IComponentData { }
public struct SlimeBeamShooterLevelUpEvent : IComponentData { }
public struct RadiantFieldLevelUpEvent : IComponentData { }
public struct PawPrintPoisonerLevelUpEvent : IComponentData { }

// Passive Level Up Events
public struct MaxHealthLevelUpEvent : IComponentData { }
public struct ArmorLevelUpEvent : IComponentData { }
public struct MoveSpeedLevelUpEvent : IComponentData { }
public struct HealthRegenLevelUpEvent : IComponentData { }
public struct PickupRadiusLevelUpEvent : IComponentData { }
public struct AbilityHasteLevelUpEvent : IComponentData { }
public struct DamageLevelUpEvent : IComponentData { }

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

public struct PlayerUpgradeSlots : IComponentData
{
    public FixedList64Bytes<int> WeaponIDs;
    public FixedList64Bytes<int> PassiveIDs; 
}