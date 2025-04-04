using Unity.Entities;
using UnityEngine;

public struct InitializationTrackerComponent : IComponentData 
{
    public bool healthSystemInitialized;
    public bool xpSystemInitialized;
    public bool weaponSystemInitialized;
    public bool flowFieldSystemInitialized;
}
