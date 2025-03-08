using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class ShootSlimeBullet : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction shootSlimeBulletAction;

    private EntityManager entityManager;

    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        playerInput = GetComponent<PlayerInput>();
        shootSlimeBulletAction = playerInput.actions["Shoot Slime Bullet"];
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (shootSlimeBulletAction.WasPressedThisFrame())
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Entity bullet = BulletManager.Instance.Take();
        SetBulletPositionAndDirection(bullet);
    }

    private void SetBulletPositionAndDirection(Entity bullet)
    {
        entityManager.SetComponentData(bullet, new LocalTransform
        {
            Position = player.transform.position,
            Rotation = Quaternion.identity,
            Scale = 1f
        });

        LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(bullet);
        SlimeBulletComponent slimeBulletComponent = entityManager.GetComponentData<SlimeBulletComponent>(bullet);

        float3 mouseWorldPosition = MapManager.GetMouseWorldPosition();
        float3 moveDirection = math.normalize(mouseWorldPosition - localTransform.Position);

        entityManager.SetComponentData(bullet, new SlimeBulletComponent
        {
            isAbleToMove = true,
            moveDirection = moveDirection,
            moveSpeed = slimeBulletComponent.moveSpeed,
            distanceTraveled = 0,
            maxDistance = slimeBulletComponent.maxDistance,
            damageAmount = slimeBulletComponent.damageAmount,
        });
    }
}
