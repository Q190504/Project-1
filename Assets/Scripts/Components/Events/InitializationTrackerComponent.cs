using Unity.Entities;
using UnityEngine;

public struct InitializationTrackerComponent : IComponentData 
{
    public bool playerHealthInitialized;
    public bool playerPositionInitialized;
    public bool playerUpgradeSlotsInitialized;
    public bool playerLevelInitialized;
    public bool passivesInitialized;
    public bool weaponsInitialized;
    public bool flowFieldSystemInitialized;
    public bool hasCleanProjectiles;
    public bool hasCleanCloudList;
    public bool hasCleanEnemies;
}
