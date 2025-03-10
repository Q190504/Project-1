using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting;

[BurstCompile]
public partial struct PlayerHealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerHealthComponent>();

        if (!SystemAPI.TryGetSingleton<PlayerHealthComponent>(out PlayerHealthComponent playerHealthComponent))
            return;

        BulletManager.Instance.SetBulletPrepare(playerHealthComponent.currentHealth);
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (playerHealth, playerEntity) in SystemAPI.Query<RefRW<PlayerHealthComponent>>().WithEntityAccess())
        {
            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(playerEntity))
            {
                var damage = state.EntityManager.GetComponentData<DamageEventComponent>(playerEntity);
                playerHealth.ValueRW.currentHealth -= damage.damageAmount;

                Debug.Log($"playerHealth.ValueRW.currentHealth: {playerHealth.ValueRW.currentHealth}");


                if (playerHealth.ValueRO.currentHealth <= 0)
                {
                    //onDie action
                    Debug.Log("Player Died!");
                }

                ecb.RemoveComponent<DamageEventComponent>(playerEntity);
            }

            //Heal
            if (state.EntityManager.HasComponent<HealEventComponent>(playerEntity))
            {
                var healEventComponent = state.EntityManager.GetComponentData<HealEventComponent>(playerEntity);
                playerHealth.ValueRW.currentHealth += healEventComponent.healAmount;

                if (playerHealth.ValueRO.currentHealth >= playerHealth.ValueRO.maxHealth)
                {
                    playerHealth.ValueRW.currentHealth = playerHealth.ValueRO.maxHealth;
                }

                ecb.RemoveComponent<HealEventComponent>(playerEntity);
            }
        }
    }
}
