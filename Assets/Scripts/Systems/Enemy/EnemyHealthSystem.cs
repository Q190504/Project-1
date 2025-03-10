using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct EnemyHealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyHealthComponent>();
    }


    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var enemyEntitiesToReturn = new NativeList<Entity>(Allocator.Temp);

        foreach (var (enemyHealth, enemyEntity) in SystemAPI.Query<RefRW<EnemyHealthComponent>>().WithEntityAccess())
        {
            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(enemyEntity))
            {
                var damage = state.EntityManager.GetComponentData<DamageEventComponent>(enemyEntity);
                enemyHealth.ValueRW.currentHealth -= damage.damageAmount;

                if (enemyHealth.ValueRO.currentHealth <= 0)
                {
                    // Queue structural changes with ECB.
                    ecb.AddComponent<Disabled>(enemyEntity);
                    ecb.SetComponent(enemyEntity, new EnemyHealthComponent
                    {
                        currentHealth = enemyHealth.ValueRO.maxHealth,
                        maxHealth = enemyHealth.ValueRO.maxHealth,
                    });

                    // Collect the entity to return later.
                    enemyEntitiesToReturn.Add(enemyEntity);
                }

                ecb.RemoveComponent<DamageEventComponent>(enemyEntity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();


        for (int i = 0; i < enemyEntitiesToReturn.Length; i++)
        {
            EnemyManager.Instance.Return(enemyEntitiesToReturn[i]);
        }
        enemyEntitiesToReturn.Dispose();
    }
}
