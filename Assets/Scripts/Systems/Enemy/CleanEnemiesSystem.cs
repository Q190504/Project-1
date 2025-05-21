using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct CleanEnemiesSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.hasCleanEnemies)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Clean up enemies
            foreach (var (_, enemyEntity) in SystemAPI.Query<RefRO<EnemyTagComponent>>().WithEntityAccess())
            {
                EnemyManager.Instance.Return(enemyEntity, ecb);
            }

            // Update tracker
            tracker.hasCleanEnemies = true;
            ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);

            EnemyManager.Instance.Initialize();
        }
    }
}
