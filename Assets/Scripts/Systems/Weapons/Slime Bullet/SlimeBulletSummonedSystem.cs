using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct SlimeBulletSummonedSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBulletSummonedSystem!");
            return;
        }

        foreach (var (localTransform, slimeBulletComponent) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<SlimeBulletComponent>>())
        {
            if(slimeBulletComponent.ValueRO.isBeingSummoned)
            {
                float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;
                float3 bulletPosition = localTransform.ValueRO.Position;
                SlimeReclaimComponent slimeReclaimComponent = entityManager.GetComponentData<SlimeReclaimComponent>(player);
                float3 directionToPlayer = math.abs(localTransform.ValueRO.Position - playerPosition);
                float distanceToPlayer = math.length(directionToPlayer);

                float3 moveDirection = math.normalize(playerPosition - bulletPosition);
                localTransform.ValueRW.Position += moveDirection * slimeReclaimComponent.bulletSpeedWhenSummoned * SystemAPI.Time.DeltaTime;
            }
        }
    }
}
