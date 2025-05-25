using Unity.Entities;
using UnityEngine;

public struct InitializationTrackerComponent : IComponentData 
{
    public bool playerHealthSystemInitialized;
    public bool playerPositionSystemInitialized;
    public bool levelSystemInitialized;
    public bool weaponSystemInitialized;
    public bool flowFieldSystemInitialized;
    public bool hasCleanProjectiles;
    public bool hasCleanCloudList;
    public bool hasCleanEnemies;
}
