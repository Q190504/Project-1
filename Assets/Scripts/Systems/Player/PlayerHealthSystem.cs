using UnityEngine;
using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct PlayerHealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<PlayerHealthComponent>(out PlayerHealthComponent playerHealthComponent))
            return;

        BulletManager.Instance.SetBulletPrepare(playerHealthComponent.currentHealth);
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerHealth, playerEntity) in SystemAPI.Query<RefRW<PlayerHealthComponent>>().WithEntityAccess())
        {
            if(playerHealth.ValueRO.currentHealth <= 0)
            {
                //onDie action
            }
        }
    }
}
