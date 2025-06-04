using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.UIElements;

public partial struct PlayerAnimationSystem : ISystem
{
    private EntityManager entityManager;

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        if (!SystemAPI.ManagedAPI.TryGetSingleton(out AnimationVisualPrefabsComponent animationVisualPrefabs))
            return;

        entityManager = state.EntityManager;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, playerMovementSpeed, playerInput, playerHealth, playerTag, entity) in
            SystemAPI.Query<LocalTransform, PlayerMovementSpeedComponent, PlayerInputComponent,
            PlayerHealthComponent, PlayerTagComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject playerVisual = Object.Instantiate(animationVisualPrefabs.player);

                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = playerVisual });
            }
            else
            {
                VisualReferenceComponent playerVisualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);
                GameObject playerGO = playerVisualReference.gameObject;
                Animator playerAnimator = playerGO.GetComponent<Animator>();

                playerGO.transform.position = transform.Position;
                float3 move = new float3(playerInput.moveInput.x, playerInput.moveInput.y, 0);
                float speed = math.length(move * playerMovementSpeed.totalSpeed);
                playerAnimator.SetFloat("speed", speed);

                // Flip logic
                Vector3 currentScale = playerGO.transform.localScale;
                if (playerInput.moveInput.x > 0.01f)
                {
                    playerGO.transform.localScale = new Vector3(math.abs(currentScale.x), currentScale.y, currentScale.z);
                }
                else if (playerInput.moveInput.x < -0.01f)
                {
                    playerGO.transform.localScale = new Vector3(-math.abs(currentScale.x), currentScale.y, currentScale.z);
                }

                if (playerHealth.currentHealth <= 0)
                {
                    playerAnimator.SetTrigger("die");
                }
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}
