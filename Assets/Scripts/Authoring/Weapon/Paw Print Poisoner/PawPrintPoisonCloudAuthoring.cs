using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class PawPrintPoisonCloudAuthoring : MonoBehaviour
{
    public class Baker : Baker<PawPrintPoisonCloudAuthoring>
    {
        public override void Bake(PawPrintPoisonCloudAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PawPrintPoisonCloudComponent
            {
                tick = 0f,
                tickTimer = 0,
                damagePerTick = 0,
                cloudRadius = 0f,
                maximumCloudDuration = 0f,
                existDurationTimer = 1f,
                bonusMoveSpeedPerTargetInTheCloudModifier = 0f,
                totalEnemiesCurrentlyInTheCloud = 0,
            });
        }
    }
}

public struct PawPrintPoisonCloudComponent : IComponentData
{
    public float tick;
    public float tickTimer;
    public int damagePerTick;
    public float cloudRadius;
    public float maximumCloudDuration;
    public float existDurationTimer;
    public float bonusMoveSpeedPerTargetInTheCloudModifier;
    public int totalEnemiesCurrentlyInTheCloud;
}