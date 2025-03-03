using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity enemyPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get the baked entity prefab
        EntityQuery query = entityManager.CreateEntityQuery(typeof(EnemyPrefabComponent));
        if (query.CalculateEntityCount() > 0)
        {
            enemyPrefab = entityManager.GetComponentData<EnemyPrefabComponent>(query.GetSingletonEntity()).enemyPrefab;
        }
        else
        {
            Debug.LogError("Enemy prefab not found! Make sure it's baked correctly.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //return;
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy(new Vector3(Random.Range(-10f, 10f), 0, 0));
        }
    }

    public void SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == Entity.Null) return;

        Entity enemyInstance = entityManager.Instantiate(enemyPrefab);

        // Set the enemy position
        entityManager.SetComponentData(enemyInstance, new LocalTransform
        {
            Position = position,
            Rotation = Quaternion.identity,
            Scale = 1f
        });
    }

}
