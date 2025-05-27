using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct GameInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitializationTrackerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.GetNeedToReset()) return;

        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker))
        {
            var trackerEntity = SystemAPI.GetSingletonEntity<InitializationTrackerComponent>();

            //tracker.flowFieldSystemInitialized = false;
            tracker.playerPositionSystemInitialized = false;
            tracker.playerHealthSystemInitialized = false;
            tracker.weaponSystemInitialized = false;
            tracker.passiveSystemInitialized = false;
            tracker.levelSystemInitialized = false;
            tracker.hasCleanEnemies = false;
            tracker.hasCleanProjectiles = false;
            tracker.hasCleanCloudList = false;

            // Update tracker
            state.EntityManager.SetComponentData(trackerEntity, tracker);

            GameManager.Instance.SetNeedToReset(false);
        }
    }
}
