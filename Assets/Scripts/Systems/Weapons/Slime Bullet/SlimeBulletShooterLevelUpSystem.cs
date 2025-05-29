using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct SlimeBulletShooterLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<SlimeBulletShooterComponent>(out Entity entity))
        {
            if (state.EntityManager.HasComponent<UpgradeEvent>(entity))
            {
                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(entity);
                weaponComponent.Level += 1;

                ecb.SetComponent(entity, weaponComponent);
                ecb.RemoveComponent<UpgradeEvent>(entity);
            }
        }
    }
}
