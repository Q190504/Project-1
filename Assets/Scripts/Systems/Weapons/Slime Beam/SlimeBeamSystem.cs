using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
public partial struct SlimeBeamSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
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

        foreach (var (weapon, entity) in SystemAPI.Query<RefRW<SlimeBeamComponent>>().WithEntityAccess())
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
            float range = levelData.range;
            float timeBetween = levelData.timeBetween;

            float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

            if(level == 5) //max level
            {
                for (int slashCount = 0; slashCount < 4; slashCount++)
                    PerformSingleBeam(entity, playerPosition, range, damage, slashCount, ecb);

                slasher.timer = cooldown; // Reset timer
            }
            else
            {
                slasher.timeBetween += deltaTime;

                if (slasher.timeBetween >= timeBetween && slasher.slashCount < 4)
                {
                    PerformSingleBeam(entity, playerPosition, range, damage, slasher.slashCount, ecb);

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

    private void PerformSingleBeam(Entity entity, float3 originPosition, float range, int damage, int slashCount, EntityCommandBuffer ecb)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeList<Unity.Physics.RaycastHit> hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        float3 direction = GetAttackPosition(slashCount);

        //spawn beam

        hits.Dispose();
    }

    private float3 GetAttackPosition(int count)
    {
        float offset = 1f; // Adjust based on desired effect
        switch (count % 4)
        {
            case 0: return new float3(0, offset, 0);  // Top (0°)
            case 1: return new float3(offset, 0, 0);  // Right (-90°)
            case 2: return new float3(0, -offset, 0); // Bottom (180°)
            case 3: return new float3(-offset, 0, 0); // Left (90°)
        }
        return float3.zero;
    }
}
