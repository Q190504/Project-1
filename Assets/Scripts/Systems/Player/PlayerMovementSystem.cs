using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.InputSystem;

[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInput, playerMovement, physicsVelocity, playerTagComponent, slimeFrenzyComponent) in
                 SystemAPI.Query<RefRO<PlayerInputComponent>, RefRO<PlayerMovementComponent>, RefRW<PhysicsVelocity>, RefRO<PlayerTagComponent>, RefRO<SlimeFrenzyComponent>>())
        {
            float2 targetVelocity;
            if (playerTagComponent.ValueRO.isStunned)
                targetVelocity = new float2(0, 0);
            else if(playerTagComponent.ValueRO.isFrenzing)
                targetVelocity = playerInput.ValueRO.moveInput * (playerMovement.ValueRO.speed + playerMovement.ValueRO.speed * slimeFrenzyComponent.ValueRO.bonusMovementSpeedPercent);
            else
                targetVelocity = playerInput.ValueRO.moveInput * playerMovement.ValueRO.speed;

            physicsVelocity.ValueRW.Linear.xy = math.lerp(physicsVelocity.ValueRW.Linear.xy, targetVelocity, playerMovement.ValueRO.smoothTime);
        }
    }
}
