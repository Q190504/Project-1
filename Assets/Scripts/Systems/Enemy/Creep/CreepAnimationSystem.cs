using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
public partial struct CreepAnimationSystem : ISystem
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

        foreach (var (transform, physicsVelocity, creepTag, entity) in
            SystemAPI.Query<LocalTransform, PhysicsVelocity, CreepTagComponent>().WithEntityAccess())
        {
            // Hasnt had VisualReferenceComponent -> add
            if (!entityManager.HasComponent<VisualReferenceComponent>(entity))
            {
                GameObject creepVisual = AnimationManager.Instance.TakeCreep();

                ecb.AddComponent(entity, new VisualReferenceComponent { gameObject = creepVisual });
            }
            else
            {
                VisualReferenceComponent creepVisualReference =
                    entityManager.GetComponentData<VisualReferenceComponent>(entity);

                Animator animator = creepVisualReference.gameObject.GetComponent<Animator>();

                creepVisualReference.gameObject.transform.position = transform.Position;
                float speed = math.length(physicsVelocity.Linear);
                animator.SetFloat("speed", speed);
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}
