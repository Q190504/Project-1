using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct StartGameSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InitializationTrackerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && GameManager.Instance.IsInitializing())
        {
            if (tracker.playerPositionSystemInitialized
                && tracker.playerHealthSystemInitialized
                && tracker.weaponSystemInitialized
                && tracker.levelSystemInitialized
                && tracker.passiveSystemInitialized
                && tracker.hasCleanEnemies
                && tracker.hasCleanProjectiles
                && tracker.hasCleanCloudList)
                GameManager.Instance.SetGameState(GameState.Playing);
        }
    }
}
