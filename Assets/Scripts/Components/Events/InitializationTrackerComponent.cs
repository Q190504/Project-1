using Unity.Entities;
using UnityEngine;

public struct InitializationTrackerComponent : IComponentData 
{
    public bool playerHealthSystemInitialized;
    public bool playerPositionSystemInitialized;
    public bool xpSystemInitialized;
    public bool weaponSystemInitialized;
    public bool flowFieldSystemInitialized;
}
