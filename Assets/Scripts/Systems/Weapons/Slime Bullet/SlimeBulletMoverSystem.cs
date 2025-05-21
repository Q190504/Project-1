using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

[BurstCompile]
public partial struct SlimeBulletMoverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (localTransform, slimeBulletComponent, physicsVelocity, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<SlimeBulletComponent>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            if (!GameManager.Instance.IsPlaying())
            {
                slimeBulletComponent.ValueRW.isAbleToMove = false;
            }

            if (slimeBulletComponent.ValueRO.isAbleToMove)
            {
                float3 targetVelocity = new float3(slimeBulletComponent.ValueRO.moveDirection.x, slimeBulletComponent.ValueRO.moveDirection.y, 0) * slimeBulletComponent.ValueRO.moveSpeed;
                physicsVelocity.ValueRW.Linear = math.lerp(physicsVelocity.ValueRW.Linear, targetVelocity, 0.1f);

                slimeBulletComponent.ValueRW.distanceTraveled += slimeBulletComponent.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;
                if (slimeBulletComponent.ValueRO.distanceTraveled >= slimeBulletComponent.ValueRO.maxDistance)
                    slimeBulletComponent.ValueRW.isAbleToMove = false;
            }
            else
            {
                physicsVelocity.ValueRW.Linear = 0;

                if (slimeBulletComponent.ValueRO.existDuration <= 0)
                    ProjectilesManager.Instance.ReturnSlimeBullet(entity, ecb);
                else
                    slimeBulletComponent.ValueRW.existDuration -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}
