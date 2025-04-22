using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
public partial struct CreateWeaponSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerHealthComponent>();

        // Ensure InitializationTracker exists
        if (!SystemAPI.HasSingleton<InitializationTrackerComponent>())
        {
            var trackerEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(trackerEntity, new InitializationTrackerComponent());
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingleton<WeaponDatabaseComponent>(out var weaponDatabaseComponent))
        {
            // Track Initialization Progress
            if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker))
            {
                if (!tracker.weaponSystemInitialized)
                {
                    entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
                    if (playerQuery.CalculateEntityCount() > 0)
                        player = playerQuery.GetSingletonEntity();

                    ref var weaponDatabase = ref weaponDatabaseComponent.weaponDatabase.Value;
                    for (int i = 0; i < weaponDatabase.weapons.Length; i++)
                    {
                        ref var weapon = ref weaponDatabase.weapons[i];

                        Debug.Log($"Attach {weapon.type}");

                        // Create child entity
                        Entity childEntity = state.EntityManager.CreateEntity(typeof(LocalToWorld), typeof(LocalTransform), typeof(Parent));
                        state.EntityManager.SetComponentData(childEntity, new Parent() { Value = player });
                        state.EntityManager.SetComponentData(childEntity, new LocalToWorld() { Value = float4x4.identity });
                        state.EntityManager.SetComponentData(childEntity, LocalTransform.Identity);

                        // Attach child to player
                        entityManager.AddComponentData(childEntity, new Parent { Value = player });

                        if (weapon.type == WeaponType.SlimeBullet)
                            state.EntityManager.AddComponentData(childEntity, new WeaponComponent
                            {
                                currentLevel = 1,
                                type = weapon.type,
                            });
                        else
                            state.EntityManager.AddComponentData(childEntity, new WeaponComponent
                            {
                                currentLevel = 0,
                                type = weapon.type,
                            });
                    }

                    // Update tracker
                    tracker.weaponSystemInitialized = true;
                    ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
                }
            }
        }
    }
}