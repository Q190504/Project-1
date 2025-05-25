using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyHealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyHealthComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var enemyEntitiesToReturn = new NativeList<Entity>(Allocator.Temp);

        foreach (var (enemyHealth, localTransform, enemyEntity) in SystemAPI.Query<RefRW<EnemyHealthComponent>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(enemyEntity))
            {
                var damage = state.EntityManager.GetComponentData<DamageEventComponent>(enemyEntity);
                enemyHealth.ValueRW.currentHealth -= damage.damageAmount;

                if (enemyHealth.ValueRO.currentHealth <= 0)
                {
                    // Collect the entity to return later.
                    enemyEntitiesToReturn.Add(enemyEntity);
                    GameManager.Instance.AddEnemyKilled();

                    // Try to spawn XP orb
                    XPManager.Instance.TrySpawnExperienceOrb(localTransform.ValueRO.Position, ecb);
                }

                ecb.RemoveComponent<DamageEventComponent>(enemyEntity);
            }
        }

        for (int i = 0; i < enemyEntitiesToReturn.Length; i++)
        {
            EnemyManager.Instance.Return(enemyEntitiesToReturn[i], ecb);
        }

        enemyEntitiesToReturn.Dispose();
    }
}
