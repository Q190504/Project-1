using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial struct SlimeBeamAnimationSystem : ISystem
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

        foreach (var (transform, slimeBeamComponent, entity) in
            SystemAPI.Query<LocalTransform, SlimeBeamComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject visual = AnimationManager.Instance.TakeSlimeBeam();
                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = visual });
            }
            else
            {
                VisualReferenceComponent visualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);

                visualReference.gameObject.SetActive(true);

                Animator playerAnimator = visualReference.gameObject.GetComponent<Animator>();
                visualReference.gameObject.transform.position = transform.Position;
                visualReference.gameObject.transform.rotation = transform.Rotation;
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}

