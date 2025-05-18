using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool IsPlayerAround {  get; private set; }

    private Entity playerEntity;
    private EntityManager entityManager;
    private float detectionRadius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsPlayerAround = false;
        detectionRadius = GetComponent<CircleCollider2D>().radius;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
        if (playerQuery.CalculateEntityCount() > 0)
            playerEntity = playerQuery.GetSingletonEntity();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.GetGameState()) return;

        if (playerEntity == Entity.Null) return;

        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        Vector3 playerPosition = playerTransform.Position;

        if (Vector3.Distance(transform.position, playerPosition) <= detectionRadius)
            IsPlayerAround = true;
        else
            IsPlayerAround = false;
    }
}
