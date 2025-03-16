using Unity.Entities;
using UnityEngine;

public struct SlimeFrenzyTimerComponent : IComponentData
{
    public float timeRemaining;
    public float initialDuration;
}
