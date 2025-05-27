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
        bool isPressingEKey = false;
        bool isPressingRKey = false;
        bool isPressingEscKey = false;

        if (Keyboard.current != null)
        {
            movement.x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            movement.y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);

            isPressingEKey = Keyboard.current.eKey.wasReleasedThisFrame;
            isPressingRKey = Keyboard.current.rKey.wasReleasedThisFrame;
            isPressingEscKey = Keyboard.current.escapeKey.wasReleasedThisFrame;
        }

        //bool isShooting = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;


        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInputComponent>>())
        {
            if (GameManager.Instance.IsPlaying() == false) return;

            playerInput.ValueRW.moveInput = movement;
            //playerInput.ValueRW.isShootingPressed = isShooting;
            playerInput.ValueRW.isEPressed = isPressingEKey;
            playerInput.ValueRW.isRPressed = isPressingRKey;
            playerInput.ValueRW.isEscPressed = isPressingEscKey;
        }
    }
}
