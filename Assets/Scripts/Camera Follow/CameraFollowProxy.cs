using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CameraFollowProxy : MonoBehaviour
{
    private EntityManager entityManager;
    public Entity playerEntity;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (playerEntity == Entity.Null)
        {
            EntityQuery query = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
            if (query.CalculateEntityCount() > 0)
                playerEntity = query.GetSingletonEntity();
        }
    }


    void LateUpdate()
    {
        if (playerEntity != Entity.Null && entityManager.Exists(playerEntity))
        {
            transform.position = entityManager.GetComponentData<LocalTransform>(playerEntity).Position;
        }
    }
}
