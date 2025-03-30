using Unity.Entities;

public struct WeaponComponent : IComponentData
{
    public WeaponType type;
    public int currentLevel; // Starts at 0 (Level 0 == disable)
}
