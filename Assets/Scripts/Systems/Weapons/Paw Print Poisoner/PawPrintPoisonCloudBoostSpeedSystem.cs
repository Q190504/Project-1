using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(PlayerMovementSystem))]
public partial struct PawPrintPoisonCloudBoostSpeedSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private Entity pawPrintPoisoner;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
        state.RequireForUpdate<PawPrintPoisonerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in PawPrintPoisonCloudBoostSpeedSystem!");
            return;
        }

        if (!SystemAPI.TryGetSingletonEntity<PawPrintPoisonerComponent>(out pawPrintPoisoner))
        {
            Debug.Log($"Cant Found Paw Print Poisoner Entity in PawPrintPoisonCloudBoostSpeedSystem!");
            return;
        }

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        int currentLevel = entityManager.GetComponentData<PawPrintPoisonerComponent>(pawPrintPoisoner).level;
        //if(currentLevel < 5)
        //{
        //    // Not max level, no effect
        //    return;
        //}

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Collect cloud data
        var poisonClouds = new NativeList<(float3 pos, float radius)>(Allocator.Temp);
        float bonusPercentPerEnemy = 0;

        foreach (var (cloud, transform) in SystemAPI.Query<PawPrintPoisonCloudComponent, LocalTransform>())
        {
            poisonClouds.Add((transform.Position, cloud.cloudRadius));
            bonusPercentPerEnemy = cloud.bonusMoveSpeedPerTargetInTheCloudModifier;
        }

        // Count enemies in cloud
        int enemiesInCloud = 0;
        foreach (var enemyTransform in SystemAPI.Query<LocalTransform>().WithAll<EnemyTagComponent>())
        {
            float3 enemyPos = enemyTransform.Position;

            foreach (var (cloudPos, radius) in poisonClouds)
            {
                if (math.distance(enemyPos, cloudPos) < radius)
                {
                    enemiesInCloud++;
                    break;
                }
            }
        }

        // Update player speed
        PlayerMovementComponent playerMovement =
                        entityManager.GetComponentData<PlayerMovementComponent>(player);

        float bonusMultiplier = 1f + (bonusPercentPerEnemy * enemiesInCloud);
        playerMovement.currentSpeed = playerMovement.baseSpeed * bonusMultiplier;

        entityManager.SetComponentData(player, playerMovement); 

        poisonClouds.Dispose();
    }
}
