using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
public partial struct SlimeBeamShooterSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnCreate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float deltaTime = SystemAPI.Time.DeltaTime;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBeamSystem!");
            return;
        }

        foreach (var (weapon, entity) in SystemAPI.Query<RefRW<SlimeBeamShooterComponent>>().WithEntityAccess())
        {
            ref var slasher = ref weapon.ValueRW;
            slasher.timer -= deltaTime;
            if (slasher.timer > 0) continue;

            var blobData = slasher.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;

            // Determine weapon level index (can be from another component if you support dynamic leveling)
            int levelIndex = 1;
            if (levelIndex <= 0) // is active
            {
                Debug.Log($"SlimeSlash is inactive");
                return;
            }

            ref var levelData = ref blobData.Value.Levels[levelIndex];

            int level = levelData.level;
            int damage = levelData.damage;
            float cooldown = levelData.cooldown;
            float timeBetween = levelData.timeBetween;

            float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

            if(level == 5) //max level
            {
                for (int slashCount = 0; slashCount < 4; slashCount++)
                    PerformSingleBeam(entity, playerPosition, damage, slashCount, ecb);

                slasher.timer = cooldown; // Reset timer
            }
            else
            {
                slasher.timeBetween += deltaTime;

                if (slasher.timeBetween >= timeBetween && slasher.slashCount < 4)
                {
                    PerformSingleBeam(entity, playerPosition, damage, slasher.slashCount, ecb);

                    slasher.slashCount++;
                    slasher.timeBetween = 0f;
                }

                if (slasher.slashCount >= 4)
                {
                    slasher.slashCount = 0;
                    slasher.timer = cooldown; // Reset timer
                }
            }
        }
    }

    private void PerformSingleBeam(Entity entity, float3 originPosition, int damage, int beamCount, EntityCommandBuffer ecb)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeList<Unity.Physics.RaycastHit> hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        float3 direction = GetAttackPosition(beamCount);
        Quaternion rotation = GetRotation(beamCount);

        //spawn beam
        Entity slimeBeamInstance = BulletManager.Instance.TakeSlimeBeam(ecb);
        SetStats(ecb, slimeBeamInstance, damage, originPosition, rotation);

        hits.Dispose();
    }

    private void SetStats(EntityCommandBuffer ecb, Entity beam, int damage, float3 originPosition, quaternion rotation)
    {
        ecb.SetComponent(beam, new LocalTransform
        {
            Position = originPosition,
            Rotation = rotation,
            Scale = 1f
        });

        ecb.AddComponent(beam, new SlimeBeamComponent
        {
            damage = damage,
            hasDealDamageToEnemies = false,
            existDuration = 0.3f,
            timer = 0
        });
    }

    private float3 GetAttackPosition(int count)
    {
        float offset = 1f;
        switch (count % 4)
        {
            case 0: return new float3(0, offset, 0);  // Top 
            case 1: return new float3(offset, 0, 0);  // Right 
            case 2: return new float3(0, -offset, 0); // Bottom
            case 3: return new float3(-offset, 0, 0); // Left 
        }
        return float3.zero;
    }

    private Quaternion GetRotation(int count)
    {
        switch (count % 4)
        {
            case 0: return Quaternion.identity;         // Top 0 degrees
            case 1: return Quaternion.Euler(0, 0, 90);  // Right 90 degrees
            case 2: return Quaternion.Euler(0, 0, 180); // Bottom 180 degrees
            case 3: return Quaternion.Euler(0, 0, 270); // Left 270 degrees
            default: return Quaternion.identity;
        }
    }
}
