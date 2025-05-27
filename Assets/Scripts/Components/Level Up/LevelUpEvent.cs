using Unity.Collections;
using Unity.Entities;

public struct LevelUpEvent : IComponentData { }

public struct WeaponComponent : IComponentData
{
    public int ID;
    public int Level;
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
}

public struct PassiveComponent : IComponentData
{
    public int ID;
    public int Level;
    public FixedString128Bytes DisplayName;
    public FixedString512Bytes Description;
}

public struct PlayerUpgradeSlots : IComponentData
{
    public FixedList64Bytes<int> WeaponIDs;
    public FixedList64Bytes<int> PassiveIDs; 
}