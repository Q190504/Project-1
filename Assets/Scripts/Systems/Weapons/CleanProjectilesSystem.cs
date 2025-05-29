using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct CleanProjectilesSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.hasCleanProjectiles)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Clean up projectiles
            foreach (var (_, slimeBulletEntity) in SystemAPI.Query<RefRO<SlimeBulletComponent>>().WithEntityAccess())
            {
                ProjectilesManager.Instance.ReturnSlimeBullet(slimeBulletEntity, ecb);
            }

            foreach (var (_, slimeBeamEntity) in SystemAPI.Query<RefRO<SlimeBeamComponent>>().WithEntityAccess())
            {
                ProjectilesManager.Instance.ReturnSlimeBeam(slimeBeamEntity, ecb);
            }

            foreach (var (_, poisonCloudEntity) in SystemAPI.Query<RefRO<PawPrintPoisonCloudComponent>>().WithEntityAccess())
            {
                ProjectilesManager.Instance.ReturnPoisonCloud(poisonCloudEntity, ecb);
            }

            foreach (var (_, experienceOrbEntity) in SystemAPI.Query<RefRO<ExperienceOrbComponent>>().WithEntityAccess())
            {
                ExperienceOrbManager.Instance.Return(experienceOrbEntity, ecb);
            }

            // Update tracker
            tracker.hasCleanProjectiles = true;

            ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
        }
    }
}
