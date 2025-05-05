using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInput, playerMovement, physicsVelocity, playerTagComponent, slimeFrenzyComponent) in
                 SystemAPI.Query<RefRO<PlayerInputComponent>, RefRO<PlayerMovementComponent>, RefRW<PhysicsVelocity>, RefRO<PlayerTagComponent>, RefRO<SlimeFrenzyComponent>>())
        {
            float3 targetVelocity;
            if (playerTagComponent.ValueRO.isStunned)
                targetVelocity = float3.zero;
            else if (playerTagComponent.ValueRO.isFrenzing)
                targetVelocity = new float3(playerInput.ValueRO.moveInput.x, playerInput.ValueRO.moveInput.y, 0) * (playerMovement.ValueRO.speed + playerMovement.ValueRO.speed * slimeFrenzyComponent.ValueRO.bonusMovementSpeedPercent);
            else
                targetVelocity = new float3(playerInput.ValueRO.moveInput.x, playerInput.ValueRO.moveInput.y, 0) * playerMovement.ValueRO.speed;

            physicsVelocity.ValueRW.Linear = math.lerp(physicsVelocity.ValueRW.Linear, targetVelocity, playerMovement.ValueRO.smoothTime);
        }
    }
}
