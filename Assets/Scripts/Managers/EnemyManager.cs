using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    [SerializeField] private int enemyPrepare;
    public List<EnemySpawner> SpawnerList = new List<EnemySpawner>();

    private EntityManager entityManager;
    private Entity enemyPrefab;

    private NativeQueue<Entity> inactiveEnemies;
    private int enemyCount = 0;

    public int enemiesPerWave; // Number of enemies per wave

    public float initialDelay; // Time before the first wave
    public float waveInterval; // Time between waves
    private float waveTimer;
    public float spawnDelay; // Delay between spawning each enemy in a wave
    private float delayTimer = 0;

    public static EnemyManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<EnemyManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        inactiveEnemies = new NativeQueue<Entity>(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (inactiveEnemies.IsCreated)
            inactiveEnemies.Dispose();
    }

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

        waveTimer = initialDelay; 
        delayTimer = 0;
        PrepareEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (waveTimer <= 0)
        {
            if(delayTimer <= 0)
            {
                foreach (EnemySpawner enemySpawner in SpawnerList)
                {
                    if (enemySpawner.IsPlayerAround)
                    {
                        SpawnEnemy(enemySpawner.transform.position);
                    }
                }

                delayTimer = spawnDelay;
            }
            else
                delayTimer -= Time.deltaTime;

            waveTimer = waveInterval;
        }
        else
            waveTimer -= Time.deltaTime;
    }

    public void SpawnEnemy(Vector3 position)
    {
        Entity enemyInstance = Take();

        // Set the enemy position
        entityManager.SetComponentData(enemyInstance, new LocalTransform
        {
            Position = position,
            Rotation = Quaternion.identity,
            Scale = 1f
        });
    }

    private void PrepareEnemy()
    {
        if (enemyPrefab == Entity.Null) return;

        for (int i = 0; i < enemyPrepare; i++)
        {
            Entity slimeBulletInstance = entityManager.Instantiate(enemyPrefab);
            entityManager.AddComponent<Disabled>(slimeBulletInstance);
            inactiveEnemies.Enqueue(slimeBulletInstance);
            enemyCount++;
        }
    }
    public Entity Take()
    {
        if (inactiveEnemies.IsEmpty())
            PrepareEnemy();

        Entity slimeBulletInstance = inactiveEnemies.Dequeue();
        enemyCount--;
        entityManager.RemoveComponent<Disabled>(slimeBulletInstance);
        return slimeBulletInstance;
    }

    public void Return(Entity bullet)
    {
        if (!entityManager.Exists(bullet)) return;

        entityManager.AddComponent<Disabled>(bullet);
        inactiveEnemies.Enqueue(bullet);
        enemyCount++;
    }
}
