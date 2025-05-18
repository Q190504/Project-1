using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(PlayerMovementSystem))]
public partial struct PawPrintPoisonerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private Entity pawPrintPoisonerEntity;
    private NativeList<Entity> clouds;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonerComponent>();
        clouds = new NativeList<Entity>(24, Allocator.Persistent); // Initialize the list with a capacity of the maximum number of clouds
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.GetGameState()) return;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in PawPrintPoisonerSystem!");
            return;
        }

        if (!SystemAPI.TryGetSingletonEntity<PawPrintPoisonerComponent>(out pawPrintPoisonerEntity))
        {
            Debug.Log($"Cant Found Paw Print Poisoner Entity in PawPrintPoisonerSystem!");
            return;
        }

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        float deltaTime = SystemAPI.Time.DeltaTime;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        PawPrintPoisonerComponent pawPrintPoisoner = entityManager.GetComponentData<PawPrintPoisonerComponent>(pawPrintPoisonerEntity);

        pawPrintPoisoner.timer -= deltaTime;
        if (pawPrintPoisoner.timer > 0) return;

        ref var blobData = ref pawPrintPoisoner.Data;
        if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) return;

        // Determine pawPrintPoisonerComponent level
        int level = pawPrintPoisoner.level;

        if (level <= 0) // is active
        {
            return;
        }

        // Take Paw Print Poisoner's data
        float cooldown = pawPrintPoisoner.cooldown;
        float tick = pawPrintPoisoner.tick;
        float distanceToCreateACloud = pawPrintPoisoner.distanceToCreateACloud;
        float distanceTraveled = pawPrintPoisoner.distanceTraveled;

        int maximumClouds = pawPrintPoisoner.maximumClouds;

        // Take cloud's level data
        ref var levelData = ref blobData.Value.Levels[level];
        int damagePerTick = levelData.damagePerTick;
        float cloudRadius = levelData.cloudRadius;
        float maximumCloudDuration = levelData.maximumCloudDuration;
        float bonusMoveSpeedPerTargetInTheCloudModifier = levelData.bonusMoveSpeedPerTargetInTheCloudModifier;

        // Update distance traveled
        PlayerMovementComponent playerMovementComponent = entityManager.GetComponentData<PlayerMovementComponent>(player);
        float playerCurrentSpeed = playerMovementComponent.currentSpeed;
        float distanceThisFrame = playerCurrentSpeed * deltaTime;
        distanceTraveled += distanceThisFrame;

        // If the player moved at least distanceToCreateACloud & is not in any existing cloud, create a new one
        if (distanceTraveled >= distanceToCreateACloud)
        {
            // Check if the player is in any existing cloud
            bool isNotInCloud = true;
            float3 playerPos = entityManager.GetComponentData<LocalTransform>(player).Position;
            foreach (var (pawPrintPoisonCloudComponent, cloudTransform, poisonCloudEntity) in SystemAPI.Query<RefRO<PawPrintPoisonCloudComponent>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                float3 cloudPos = cloudTransform.ValueRO.Position;
                float distance = math.distance(playerPos, cloudPos);
                if (distance < pawPrintPoisonCloudComponent.ValueRO.cloudRadius)
                {
                    isNotInCloud = false;
                    break;
                }
            }
            
            // If the player is in any existing cloud, do not create a new one
            if (!isNotInCloud)
            {
                return;
            }

            // If total number of clouds is greater than maximumClouds, remove the oldest cloud
            if (clouds.Length >= maximumClouds)
            {
                Entity oldestCloud = clouds[0];

                clouds.RemoveAt(0);

                ProjectilesManager.Instance.ReturnPoisonCloud(oldestCloud, ecb);
            }

            // Spawn the cloud
            Entity cloudEntity = ProjectilesManager.Instance.TakePoisonCloud(ecb);

            // Set the cloud's stats
            SetCloudStats(ecb, cloudEntity, tick, damagePerTick, cloudRadius, maximumCloudDuration,
                bonusMoveSpeedPerTargetInTheCloudModifier);

            // Add this cloud to the list of clouds
            if (!clouds.Contains(cloudEntity))
                clouds.Add(cloudEntity);

            pawPrintPoisoner.timer = cooldown; // Reset timer
            distanceTraveled = 0; // Reset distance traveled
        }

        pawPrintPoisoner.distanceTraveled = distanceTraveled;

        ecb.SetComponent(pawPrintPoisonerEntity, pawPrintPoisoner);
    }

    public void OnDestroy(ref SystemState state)
    {
        clouds.Dispose();
    }

    public void SetCloudStats(EntityCommandBuffer ecb, Entity cloud, float tick, int damagePerTick, float cloudRadius,
        float maximumCloudDuration, float bonusMoveSpeedPerTargetInTheCloudModifier)
    {
        float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

        ecb.SetComponent(cloud, new LocalTransform
        {
            Position = playerPosition,
            Rotation = Quaternion.identity,
            Scale = cloudRadius
        });

        if (!entityManager.HasComponent<PawPrintPoisonCloudComponent>(cloud))
        {
            ecb.AddComponent(cloud, new PawPrintPoisonCloudComponent
            {
                tick = tick,
                tickTimer = tick,
                damagePerTick = damagePerTick,
                cloudRadius = cloudRadius,
                maximumCloudDuration = maximumCloudDuration,
                existDurationTimer = maximumCloudDuration,
                bonusMoveSpeedPerTargetInTheCloudModifier = bonusMoveSpeedPerTargetInTheCloudModifier,
                totalEnemiesCurrentlyInTheCloud = 0,
            });
        }
        else
        {
            ecb.SetComponent(cloud, new PawPrintPoisonCloudComponent
            {
                tick = tick,
                tickTimer = tick,
                damagePerTick = damagePerTick,
                cloudRadius = cloudRadius,
                maximumCloudDuration = maximumCloudDuration,
                existDurationTimer = maximumCloudDuration,
                bonusMoveSpeedPerTargetInTheCloudModifier = bonusMoveSpeedPerTargetInTheCloudModifier,
                totalEnemiesCurrentlyInTheCloud = 0,
            });
        }
    }
}
