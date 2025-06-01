using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public partial struct RadiantFieldAnimationSystem : ISystem
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

        foreach (var (transform, radiantField, weaponComponent, entity) in
            SystemAPI.Query<LocalTransform, RadiantFieldComponent, WeaponComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject radiantFieldVisual = Object.Instantiate(animationVisualPrefabs.radiantField);
                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = radiantFieldVisual });
            }
            else
            {
                VisualReferenceComponent visualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);

                if (weaponComponent.Level == 0)
                    visualReference.gameObject.SetActive(false);
                else
                {
                    visualReference.gameObject.SetActive(true);

                    Animator playerAnimator = visualReference.gameObject.GetComponent<Animator>();
                    visualReference.gameObject.transform.position = transform.Position;

                    // Set scale
                    var blobData = radiantField.Data;
                    if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;
                    int currentLevel = weaponComponent.Level;
                    ref var levelData = ref blobData.Value.Levels[currentLevel];
                    float offset = 0.5f; // offset of the sprite
                    float newRadius = levelData.radius + offset;
                    visualReference.gameObject.transform.localScale = Vector2.one * newRadius;
                }
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}

