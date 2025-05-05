using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct RadiantFieldTickSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;

        state.RequireForUpdate<RadiantFieldComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        Entity radiantFieldEntity = SystemAPI.GetSingletonEntity<RadiantFieldComponent>();

        RadiantFieldComponent radiantFieldComponent = entityManager.GetComponentData<RadiantFieldComponent>(radiantFieldEntity);

        // Skip if pass a tick
        if (currentTime - radiantFieldComponent.lastTickTime >= radiantFieldComponent.timeBetween)
        {
            radiantFieldComponent.lastTickTime = currentTime;
            ecb.SetComponent(radiantFieldEntity, radiantFieldComponent);
        }
    }
}
