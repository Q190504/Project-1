using Unity.Collections;
using Unity.Entities;

public partial struct EnemyHealthSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (enemyHealth, entity) in SystemAPI.Query<RefRO<EnemyHealthComponent>>().WithEntityAccess())
        {
            if (enemyHealth.ValueRO.currentHealth <= 0)
            {
                ecb.AddComponent<Disabled>(entity);

                ecb.SetComponent(entity, new EnemyHealthComponent
                {
                    currentHealth = enemyHealth.ValueRO.maxHealth,
                    maxHealth = enemyHealth.ValueRO.maxHealth,
                });

                //Return enmey
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}




