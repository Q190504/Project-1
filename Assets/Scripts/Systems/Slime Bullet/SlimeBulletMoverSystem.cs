using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Collections;

[BurstCompile]
public partial struct SlimeBulletMoverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (localTransform, slimeBulletComponent) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<SlimeBulletComponent>>().WithAbsent<Disabled>())
        {
            if (slimeBulletComponent.ValueRO.isAbleToMove)
            {
                localTransform.ValueRW.Position += slimeBulletComponent.ValueRO.moveDirection * slimeBulletComponent.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;
                slimeBulletComponent.ValueRW.distanceTraveled += slimeBulletComponent.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;

                if (slimeBulletComponent.ValueRO.distanceTraveled >= slimeBulletComponent.ValueRO.maxDistance)
                    slimeBulletComponent.ValueRW.isAbleToMove = false;
            }
        }
    }
}
