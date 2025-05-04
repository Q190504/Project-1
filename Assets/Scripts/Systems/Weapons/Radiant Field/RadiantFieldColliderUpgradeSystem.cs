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
            ref var radiantField = ref weapon.ValueRW;
            var blobData = radiantField.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;

            // Determine weapon level
            int currentLevel = radiantField.currentLevel;
            int previousLevel = radiantField.previousLevel;

            if (currentLevel <= 0) // is inactive
            {
                Debug.Log($"Radiant Field is inactive");
                return;
            }

            if (currentLevel == previousLevel) // has not level up
                continue;

            ref var levelData = ref blobData.Value.Levels[currentLevel];

            float newRadius = levelData.radius;

            localTransform.ValueRW.Scale = newRadius;

            // Update tracker
            radiantField.previousLevel = radiantField.currentLevel;
        }
    }
}
