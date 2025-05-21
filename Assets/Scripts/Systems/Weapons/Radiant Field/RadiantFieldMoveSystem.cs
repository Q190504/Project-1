using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(PlayerMovementSystem))]
public partial struct RadiantFieldMoveSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var deltaTime = SystemAPI.Time.DeltaTime;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in RadiantFieldMoveSystem!");
            return;
        }

        foreach (var (localTransform, radiantFieldComponent) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RadiantFieldComponent>>())
        {
            float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;
            float smoothTime = entityManager.GetComponentData<PlayerMovementComponent>(player).smoothTime;

            float followSpeed = 15f / smoothTime;

            localTransform.ValueRW.Position = math.lerp(
                localTransform.ValueRW.Position,
                playerPosition,
                deltaTime * followSpeed
            );
        }
    }
}
