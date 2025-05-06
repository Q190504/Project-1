using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerMovementSystem))]
public partial struct PawPrintPoisonerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private Entity pawPrintPoisonerEntity;
    private NativeList<Entity> clouds;

    public void OnCreate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        clouds = new NativeList<Entity>(24, Allocator.Persistent); // Initialize the list with a capacity of the maximum number of clouds
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        float deltaTime = SystemAPI.Time.DeltaTime;

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

        PawPrintPoisonerComponent pawPrintPoisonerComponent = entityManager.GetComponentData<PawPrintPoisonerComponent>(pawPrintPoisonerEntity);

        ref var pawPrintPoisoner = ref pawPrintPoisonerComponent;
        pawPrintPoisoner.timer -= deltaTime;
        if (pawPrintPoisoner.timer > 0) return;

        var blobData = pawPrintPoisoner.Data;
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
        float cloudSize = levelData.cloudSize;
        float maximumCloudDuration = levelData.maximumCloudDuration;
        float bonusMoveSpeedPerTargetInTheCloudModifier = levelData.bonusMoveSpeedPerTargetInTheCloud;

        // Update distance traveled
        PhysicsVelocity playerVelocityComponent = entityManager.GetComponentData<PhysicsVelocity>(player);
        float3 playerCurrentVelocity = playerVelocityComponent.Linear;
        float distanceThisFrame = math.length(playerCurrentVelocity) * deltaTime;
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
                if (distance < pawPrintPoisonCloudComponent.ValueRO.cloudSize)
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
            SetCloudStats(ecb, cloudEntity, tick, damagePerTick, cloudSize, maximumCloudDuration,
                bonusMoveSpeedPerTargetInTheCloudModifier);

            // Add this cloud to the list of clouds
            if (!clouds.Contains(cloudEntity))
                clouds.Add(cloudEntity);

            pawPrintPoisoner.timer = cooldown; // Reset timer
            distanceTraveled = 0; // Reset distance traveled
        }

        pawPrintPoisoner.distanceTraveled = distanceTraveled;

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    public void OnDestroy(ref SystemState state)
    {
        clouds.Dispose();
    }

    public void SetCloudStats(EntityCommandBuffer ecb, Entity cloud, float tick, int damagePerTick, float cloudSize,
        float maximumCloudDuration, float bonusMoveSpeedPerTargetInTheCloudModifier)
    {
        float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

        ecb.SetComponent(cloud, new LocalTransform
        {
            Position = playerPosition,
            Rotation = Quaternion.identity,
            Scale = 1f
        });

        if (!entityManager.HasComponent<PawPrintPoisonCloudComponent>(cloud))
        {
            ecb.AddComponent(cloud, new PawPrintPoisonCloudComponent
            {
                tick = tick,
                lastTick = tick,
                damagePerTick = damagePerTick,
                cloudSize = cloudSize,
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
                damagePerTick = damagePerTick,
                cloudSize = cloudSize,
                maximumCloudDuration = maximumCloudDuration,
                existDurationTimer = maximumCloudDuration,
                bonusMoveSpeedPerTargetInTheCloudModifier = bonusMoveSpeedPerTargetInTheCloudModifier,
                totalEnemiesCurrentlyInTheCloud = 0,
            });
        }
    }
}
