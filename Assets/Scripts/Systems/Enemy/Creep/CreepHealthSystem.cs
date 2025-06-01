using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct CreepHealthSystem : ISystem
{
    EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CreepHealthComponent>();
        entityManager = state.EntityManager;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var creepEntitiesToReturn = new NativeList<Entity>(Allocator.Temp);

        foreach (var (health, localTransform, enemyEntity) in SystemAPI.Query<RefRW<CreepHealthComponent>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(enemyEntity))
            {
                // Create Hit effect
                GameObject hitEffect = AnimationManager.Instance.TakeHitEffect();
                hitEffect.transform.position = localTransform.ValueRO.Position;

                var damage = state.EntityManager.GetComponentData<DamageEventComponent>(enemyEntity);
                health.ValueRW.currentHealth -= damage.damageAmount;

                if (health.ValueRO.currentHealth <= 0)
                {
                    // Collect the entity to return later.
                    creepEntitiesToReturn.Add(enemyEntity);
                    GameManager.Instance.AddEnemyKilled();

                    // Try to spawn XP orb
                    ExperienceOrbManager.Instance.TrySpawnExperienceOrb(localTransform.ValueRO.Position, ecb);
                }

                ecb.RemoveComponent<DamageEventComponent>(enemyEntity);
            }
        }

        for (int i = 0; i < creepEntitiesToReturn.Length; i++)
        {
            // Return the visual game object
            if (entityManager.HasComponent<VisualReferenceComponent>(creepEntitiesToReturn[i]))
            {
                VisualReferenceComponent visualReferenceComponent = 
                    entityManager.GetComponentData<VisualReferenceComponent>(creepEntitiesToReturn[i]);
                AnimationManager.Instance.ReturnCreep(visualReferenceComponent.gameObject);
            }

            // Return the entity
            EnemyManager.Instance.Return(creepEntitiesToReturn[i], ecb);
        }

        creepEntitiesToReturn.Dispose();
    }
}
