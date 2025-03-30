using UnityEngine;
using Unity.Entities;
using Unity.Collections;

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

                        state.EntityManager.AddComponentData(player, new WeaponComponent
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