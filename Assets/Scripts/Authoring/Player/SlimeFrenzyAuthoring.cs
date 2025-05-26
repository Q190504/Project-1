using Unity.Entities;
using UnityEngine;

public class SlimeFrenzyAuthoring : MonoBehaviour
{
    public float duration;
    public float cooldownTime;
    [Range(0f, 1f)]
    public float fireRateReductionPercent;
    [Range(0f, 1f)]
    public float bonusDamagePercent;
    [Range(0f, 1f)]
    public float bonusMovementSpeedPercent;
    [Range(1f, 5f)]
    public float hpCostPerShotPercent;


    class SlimeFrenzyBaker : Baker<SlimeFrenzyAuthoring>
    {
        public override void Bake(SlimeFrenzyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SlimeFrenzyComponent
            {
                isActive = false,
                duration = authoring.duration,
                cooldownTime = authoring.cooldownTime,
                fireRateReductionPercent = authoring.fireRateReductionPercent,
                bonusDamagePercent = authoring.bonusDamagePercent,
                bonusMovementSpeedPercent = authoring.bonusMovementSpeedPercent,
                hpCostPerShotPercent = authoring.hpCostPerShotPercent,
            });
        }
    }
}

public struct SlimeFrenzyComponent : IComponentData
{
    public bool isActive;
    public float duration;
    public float cooldownTime;
    public float fireRateReductionPercent;
    public float bonusDamagePercent;
    public float bonusMovementSpeedPercent;
    public float hpCostPerShotPercent;
}
