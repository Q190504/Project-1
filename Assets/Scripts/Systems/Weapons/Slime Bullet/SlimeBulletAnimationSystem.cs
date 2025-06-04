using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial struct SlimeBulletAnimationSystem : ISystem
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

        foreach (var (transform, slimeBulletComponent, entity) in
            SystemAPI.Query<LocalTransform, SlimeBulletComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject visual = AnimationManager.Instance.TakeSlimeBulletSlowZone();
                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = visual });
            }
            else
            {
                VisualReferenceComponent visualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);

                // Set Slow Zone
                if (slimeBulletComponent.slowModifier > 0 
                    && !slimeBulletComponent.isAbleToMove 
                    && !slimeBulletComponent.isBeingSummoned)
                {
                    visualReference.gameObject.SetActive(true);

                    Animator playerAnimator = visualReference.gameObject.GetComponent<Animator>();
                    visualReference.gameObject.transform.position = transform.Position;
                    float scale = slimeBulletComponent.slowRadius + 0.5f;
                    visualReference.gameObject.transform.localScale = scale * Vector3.one;
                }
                else
                {
                    visualReference.gameObject.SetActive(false);
                }
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}


