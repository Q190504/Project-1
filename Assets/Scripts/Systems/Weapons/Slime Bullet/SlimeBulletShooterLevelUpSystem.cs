using Unity.Entities;
using UnityEngine;

public partial struct SlimeBulletShooterLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<SlimeBeamShooterComponent>(out Entity entity))
        {
            if (state.EntityManager.HasComponent<LevelUpComponent>(entity))
            {
                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(entity);
                weaponComponent.Level += 1;

                ecb.SetComponent(entity, weaponComponent);
                ecb.RemoveComponent<LevelUpComponent>(entity);
            }
        }
    }
}
