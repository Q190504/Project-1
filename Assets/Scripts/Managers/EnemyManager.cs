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



        //for (int i = 0; i < 1000; i++)
        //{
        //    {
        //        SpawnEnemy(new Vector3(Random.Range(0f, 500f), Random.Range(0f, 500f), 0));
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {

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
