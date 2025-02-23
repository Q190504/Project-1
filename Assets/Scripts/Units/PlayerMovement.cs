using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private Vector2 currVel = Vector2.zero;
    private Vector2 maxVel;

    [SerializeField] private float speed;
    [SerializeField] private VoidPublisherSO moveEvent;
    [SerializeField] private float smoothTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Walk"];
    }

    private void FixedUpdate()
    {
        maxVel = moveAction.ReadValue<Vector2>() * speed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, maxVel, ref currVel, smoothTime);
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
