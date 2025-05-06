using Unity.Entities;
using UnityEngine;

public class PawPrintPoisonCloudAuthoring : MonoBehaviour
{
    public class Baker : Baker<PawPrintPoisonCloudAuthoring>
    {
        public override void Bake(PawPrintPoisonCloudAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PawPrintPoisonCloudComponent
            {
                damagePerTick = 0,
                cloudSize = 0f,
                maximumCloudDuration = 0f,
                existDurationTimer = 0f,
                tick = 0f,
                tickTimer = 0,
                bonusMoveSpeedPerTargetInTheCloudModifier = 0f
            });
        }
    }
}

public struct PawPrintPoisonCloudComponent : IComponentData
{
    public float tick;
    public float tickTimer;
    public int damagePerTick;
    public float cloudSize;
    public float maximumCloudDuration;
    public float existDurationTimer;
    public float bonusMoveSpeedPerTargetInTheCloudModifier;
}