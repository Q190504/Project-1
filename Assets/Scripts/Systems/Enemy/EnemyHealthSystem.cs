using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct EnemyHealthSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var enemyEntitiesToReturn = new NativeList<Entity>(Allocator.Temp);

        foreach (var (enemyHealth, entity) in SystemAPI.Query<RefRO<EnemyHealthComponent>>().WithEntityAccess())
        {
            if (enemyHealth.ValueRO.currentHealth <= 0)
            {
                // Queue structural changes with ECB.
                ecb.AddComponent<Disabled>(entity);
                ecb.SetComponent(entity, new EnemyHealthComponent
                {
                    currentHealth = enemyHealth.ValueRO.maxHealth,
                    maxHealth = enemyHealth.ValueRO.maxHealth,
                });
                // Collect the entity to return later.
                enemyEntitiesToReturn.Add(entity);
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
