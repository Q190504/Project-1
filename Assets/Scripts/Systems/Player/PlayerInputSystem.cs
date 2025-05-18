using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float2 movement = float2.zero;
        bool isPressingCKey = false;
        bool isPressingSpaceKey = false;
        bool isPressingEscKey = false;

        if (Keyboard.current != null)
        {
            movement.x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            movement.y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);

            isPressingCKey = Keyboard.current.cKey.wasReleasedThisFrame;
            isPressingSpaceKey = Keyboard.current.spaceKey.wasReleasedThisFrame;
            isPressingEscKey = Keyboard.current.escapeKey.wasReleasedThisFrame;
        }

        //bool isShooting = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;


        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInputComponent>>())
        {
            playerInput.ValueRW.moveInput = movement;
            //playerInput.ValueRW.isShootingPressed = isShooting;
            playerInput.ValueRW.isCPressed = isPressingCKey;
            playerInput.ValueRW.isSpacePressed = isPressingSpaceKey;
            playerInput.ValueRW.isEscPressed = isPressingEscKey;
        }
    }
}
