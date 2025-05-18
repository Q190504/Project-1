using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    [SerializeField] private int enemyPrepare;
    public List<EnemySpawner> SpawnerList = new List<EnemySpawner>();

    private Entity player;
    private EntityManager entityManager;
    private Entity enemyPrefab;

    private NativeQueue<Entity> inactiveEnemies;
    private int enemyCount = 0;

    public int enemiesPerWave; // Number of enemies per wave
    private int enemiesToSpawnCounter;

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

        EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
        if (playerQuery.CalculateEntityCount() > 0)
            player = playerQuery.GetSingletonEntity();

        // Get the baked entity prefab
        EntityQuery enemyPrefabQuery = entityManager.CreateEntityQuery(typeof(EnemyPrefabComponent));
        if (enemyPrefabQuery.CalculateEntityCount() > 0)
        {
            enemyPrefab = entityManager.GetComponentData<EnemyPrefabComponent>(enemyPrefabQuery.GetSingletonEntity()).enemyPrefab;
        }
        else
        {
            Debug.LogError("Enemy prefab not found! Make sure it's baked correctly.");
        }

        waveTimer = initialDelay; 
        delayTimer = 0;
        enemiesToSpawnCounter = enemiesPerWave;
        PrepareEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (waveTimer <= 0)
        {
            if (delayTimer <= 0 && enemiesToSpawnCounter > 0)
            {
                foreach (EnemySpawner enemySpawner in SpawnerList)
                {
                    if (enemySpawner.IsPlayerAround)
                    {
                        SpawnEnemy(new Vector3(enemySpawner.transform.position.x, enemySpawner.transform.position.y, 0));

                        enemiesToSpawnCounter--;

                        if(enemiesToSpawnCounter == 0)
                            break;
                    }
                }

                delayTimer = spawnDelay;
            }
            else
            {
                delayTimer -= Time.deltaTime;
            }

            // Reset wave when all enemies have spawned
            if (enemiesToSpawnCounter <= 0)
            {
                waveTimer = waveInterval;
                enemiesToSpawnCounter = enemiesPerWave; 
            }
        }
        else
        {
            waveTimer -= Time.deltaTime;
        }
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

        entityManager.SetComponentData(enemyInstance, new EnemyTargetComponent
        {
            targetEntity = player,
        });
    }

    private void PrepareEnemy()
    {
        if (enemyPrefab == Entity.Null) return;

        for (int i = 0; i < enemyPrepare; i++)
        {
            Entity enemy = entityManager.Instantiate(enemyPrefab);
            entityManager.AddComponent<Disabled>(enemy);
            inactiveEnemies.Enqueue(enemy);
            enemyCount++;
        }
    }

    public Entity Take()
    {
        if (inactiveEnemies.IsEmpty())
            PrepareEnemy();

        Entity enemy = inactiveEnemies.Dequeue();
        enemyCount--;
        entityManager.RemoveComponent<Disabled>(enemy);
        return enemy;
    }

    public void Return(Entity enemy)
    {
        if (!entityManager.Exists(enemy)) return;

        if (entityManager.HasComponent<StunTimerComponent>(enemy))
            entityManager.RemoveComponent<StunTimerComponent>(enemy);
        if (entityManager.HasComponent<SlowedByRadiantFieldTag>(enemy))
            entityManager.RemoveComponent<SlowedByRadiantFieldTag>(enemy);
        if (entityManager.HasComponent<SlowedBySlimeBulletTag>(enemy))
            entityManager.RemoveComponent<SlowedBySlimeBulletTag>(enemy);

        entityManager.AddComponent<Disabled>(enemy);

        inactiveEnemies.Enqueue(enemy);
        enemyCount++;

        GameManager.Instance.AddEnemyKilled();
    }
}
