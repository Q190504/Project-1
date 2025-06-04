using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial struct PoisonCloudAnimationSystem : ISystem
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

        foreach (var (transform, cloudComponent, entity) in
            SystemAPI.Query<LocalTransform, PawPrintPoisonCloudComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject cloudVisual = AnimationManager.Instance.TakePoisonCloud();

                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = cloudVisual });
            }
            else
            {
                VisualReferenceComponent cloudVisualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);
                cloudVisualReference.gameObject.transform.position = transform.Position;
                cloudVisualReference.gameObject.transform.localScale = Vector2.one * cloudComponent.cloudRadius;
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}

