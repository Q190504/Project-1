using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Collections;

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

//[BurstCompile]
//public struct EnemyTriggerJob : ITriggerEventsJob
//{
//    [ReadOnly] public ComponentLookup<EnemyTagComponent> EnemyLookup;

//    public void Execute(TriggerEvent triggerEvent)
//    {
//        Entity entityA = triggerEvent.EntityA;
//        Entity entityB = triggerEvent.EntityB;

//        bool isEntityAEnemy = EnemyLookup.HasComponent(entityA);
//        bool isEntityBEnemy = EnemyLookup.HasComponent(entityB);

//        if (isEntityAEnemy || isEntityBEnemy)
//        {
//            // One of the entities involved is an enemy
//            UnityEngine.Debug.Log($"Trigger Enter with Enemy: {entityA}, {entityB}");
//        }
//    }
//}
