using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
public partial struct RadiantFieldColliderUpgradeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RadiantFieldComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (weapon, localTransform, entity) in SystemAPI.Query<RefRW<RadiantFieldComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            var radiantField = weapon.ValueRW;
            var blobData = radiantField.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;

            // Determine weapon level
            int currentLevel = radiantField.currentLevel;
            int previousLevel = radiantField.previousLevel;

            if (currentLevel <= 0) // is active
            {
                Debug.Log($"Radiant Field is inactive");
                return;
            }
            Debug.Log($"currentLevel before {radiantField.currentLevel}");
            Debug.Log($"previousLevel before {radiantField.previousLevel}");
            if (currentLevel == previousLevel) // has not level up
                continue;

            ref var levelData = ref blobData.Value.Levels[currentLevel];

            float newRadius = levelData.radius;

            localTransform.ValueRW.Scale = newRadius;
            Debug.Log($"Radiant Field newRadius {localTransform.ValueRO.Scale}");

            // Update tracker
            radiantField.previousLevel = radiantField.currentLevel;
            //Debug.Log($"currentLevel after {radiantField.currentLevel}");
            //Debug.Log($"previousLevel after {radiantField.previousLevel}");

            ecb.SetComponent(entity, radiantField);
        }
    }
}
