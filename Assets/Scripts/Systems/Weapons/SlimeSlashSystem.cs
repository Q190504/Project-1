using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;

public partial struct SlimeSlashSystem : ISystem
{
    private EntityQuery weaponDatabaseQuery;

    public void OnCreate(ref SystemState state)
    {
        weaponDatabaseQuery = state.GetEntityQuery(ComponentType.ReadOnly<WeaponDatabaseComponent>());
    }

    public void OnUpdate(ref SystemState state)
    {
        if (weaponDatabaseQuery.IsEmpty) return;

        var weaponDatabaseComponent = SystemAPI.GetSingleton<WeaponDatabaseComponent>();
        ref var weaponDatabase = ref weaponDatabaseComponent.weaponDatabase.Value;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (weapon, transform, entity) in SystemAPI.Query<WeaponComponent, LocalTransform>().WithEntityAccess())
        {
            if (weapon.currentLevel > 0) // is active
            {
                float range = GetWeaponRange(ref weaponDatabase, WeaponType.SlimeSlash, weapon.currentLevel);
                int damage = GetWeaponDamage(ref weaponDatabase, WeaponType.SlimeSlash, weapon.currentLevel);
                float cooldown = GetWeaponCooldown(ref weaponDatabase, WeaponType.SlimeSlash, weapon.currentLevel);

                // Check if the entity has a CooldownComponent
                if (SystemAPI.HasComponent<CooldownComponent>(entity))
                {
                    ref var cooldownComponent = ref SystemAPI.GetComponentRW<CooldownComponent>(entity).ValueRW;

                    // Reduce cooldown over time
                    cooldownComponent.remainingTime -= deltaTime;

                    if (cooldownComponent.remainingTime > 0) return; // Still on cooldown
                }

                ref var slashState = ref SystemAPI.GetComponentRW<SlimeSlashComponent>(entity).ValueRW;
                // Determine position & rotation based on execution count
                float3 newPosition = GetAttackPosition(transform.Position, slashState.executionCount);
                quaternion newRotation = GetAttackRotation(slashState.executionCount);

                // Perform the attack
                PerformSlimeSlash(entity, newPosition, newRotation, range, damage, ecb);

                // Increment execution count for the next attack
                slashState.executionCount = (slashState.executionCount + 1) % 4;

                // Reset cooldown
                if (!SystemAPI.HasComponent<CooldownComponent>(entity))
                {
                    ecb.AddComponent(entity, new CooldownComponent { remainingTime = cooldown });
                }
                else
                {
                    SystemAPI.GetComponentRW<CooldownComponent>(entity).ValueRW.remainingTime = cooldown;
                }
            }
        }
    }

    private float GetWeaponRange(ref WeaponDatabase weaponDatabase, WeaponType type, int level)
    {
        for (int i = 0; i < weaponDatabase.weapons.Length; i++)
        {
            if (weaponDatabase.weapons[i].type == type)
            {
                int levelIndex = math.clamp(level, 0, 5);
                return weaponDatabase.weapons[i].levels[levelIndex].range;
            }
        }
        return 0f;
    }

    private int GetWeaponDamage(ref WeaponDatabase weaponDatabase, WeaponType type, int level)
    {
        for (int i = 0; i < weaponDatabase.weapons.Length; i++)
        {
            if (weaponDatabase.weapons[i].type == type)
            {
                int levelIndex = math.clamp(level, 0, 5);
                return weaponDatabase.weapons[i].levels[levelIndex].damage;
            }
        }
        return 0;
    }

    private float GetWeaponCooldown(ref WeaponDatabase weaponDatabase, WeaponType type, int level)
    {
        for (int i = 0; i < weaponDatabase.weapons.Length; i++)
        {
            if (weaponDatabase.weapons[i].type == type)
            {
                int levelIndex = math.clamp(level, 0, 5);
                return weaponDatabase.weapons[i].levels[levelIndex].cooldownTime;
            }
        }
        return 0f;
    }

    private void PerformSlimeSlash(Entity entity, float3 position, quaternion rotation, float range, int damage, EntityCommandBuffer ecb)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

        // Define initial local points (assuming a vertical capsule)
        float3 localPoint1 = new float3(0, -range / 2, 0);
        float3 localPoint2 = new float3(0, range / 2, 0);

        // Rotate the capsule endpoints
        float3 rotatedPoint1 = position + math.mul(rotation, localPoint1);
        float3 rotatedPoint2 = position + math.mul(rotation, localPoint2);

        physicsWorldSingleton.CapsuleCastAll(rotatedPoint1, rotatedPoint2, range / 2, float3.zero, 1, ref hits, CollisionFilter.Default);

        //Debug
        DebugCapsuleCast(rotatedPoint1, rotatedPoint2, range);

        foreach (ColliderCastHit hit in hits)
        {
            if (entityManager.HasComponent<EnemyHealthComponent>(hit.Entity))
            {
                ecb.AddComponent(hit.Entity, new DamageEventComponent
                {
                    damageAmount = damage,
                });
            }
        }

        hits.Dispose();
    }

    void DebugCapsuleCast(float3 position1, float3 position2, float range)
    {
        float radius = range / 2;

        // Draw the capsule with lines
        Debug.DrawLine(position1, position2, Color.red, 0.1f);

        // Draw spheres at capsule ends to visualize the capsule shape
        DebugDrawSphere(position1, radius, Color.green);
        DebugDrawSphere(position2, radius, Color.green);
    }

    // Helper method to draw a sphere using Debug.DrawLine
    void DebugDrawSphere(float3 center, float radius, Color color)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * math.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * math.PI * 2;

            float3 p1 = center + new float3(math.cos(angle1), math.sin(angle1), 0) * radius;
            float3 p2 = center + new float3(math.cos(angle2), math.sin(angle2), 0) * radius;

            Debug.DrawLine(p1, p2, color, 0.1f);
        }
    }

    private float3 GetAttackPosition(float3 basePosition, int count)
    {
        float offset = 1f; // Adjust based on desired effect
        switch (count % 4)
        {
            case 0: return basePosition + new float3(-offset, 0, 0); // Left (90°)
            case 1: return basePosition + new float3(0, offset, 0);  // Top (0°)
            case 2: return basePosition + new float3(offset, 0, 0);  // Right (-90°)
            case 3: return basePosition + new float3(0, -offset, 0); // Bottom (180°)
        }
        return basePosition;
    }

    private quaternion GetAttackRotation(int count)
    {
        switch (count % 4)
        {
            case 0: return quaternion.identity;  // Left
            case 1: return quaternion.RotateZ(math.radians(90));  // Top
            case 2: return quaternion.RotateZ(math.radians(180)); // Right
            case 3: return quaternion.RotateZ(math.radians(270)); // Bottom
        }
        return quaternion.identity;
    }
}
