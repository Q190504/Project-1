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

    [SerializeField] private int enemiesPerWave; // Number of enemies per wave
    private int enemiesToSpawnCounter;

    [SerializeField] private float firstWaveDelay; // Time before the first wave
    [SerializeField] private float waveInterval; // Time between waves
    private float waveTimer;
    [SerializeField] private float individualEnemyDelay;   // Delay between spawning each enemy in a wave
    private float individualEnemyDelayTimer;

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

        Initialize();
        PrepareEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (waveTimer <= 0)
        {
            if (individualEnemyDelayTimer <= 0 && enemiesToSpawnCounter > 0)
            {
                foreach (EnemySpawner enemySpawner in SpawnerList)
                {
                    if (enemySpawner.IsPlayerAround)
                    {
                        SpawnEnemy(new Vector3(enemySpawner.transform.position.x, enemySpawner.transform.position.y, 0));

                        enemiesToSpawnCounter--;

                        if (enemiesToSpawnCounter == 0)
                            break;
                    }
                }

                individualEnemyDelayTimer = individualEnemyDelay;
            }
            else
            {
                individualEnemyDelayTimer -= Time.deltaTime;
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
        if (!GameManager.Instance.IsPlaying())
            return;

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

        EnemyHealthComponent enemyHealthComponent = entityManager.GetComponentData<EnemyHealthComponent>(enemyInstance);

        entityManager.SetComponentData(enemyInstance, new EnemyHealthComponent
        {
            currentHealth = enemyHealthComponent.maxHealth,
            maxHealth = enemyHealthComponent.maxHealth,
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

    public void Return(Entity enemy, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(enemy)) return;

        if (entityManager.HasComponent<StunTimerComponent>(enemy))
            ecb.RemoveComponent<StunTimerComponent>(enemy);
        if (entityManager.HasComponent<SlowedByRadiantFieldTag>(enemy))
            ecb.RemoveComponent<SlowedByRadiantFieldTag>(enemy);
        if (entityManager.HasComponent<SlowedBySlimeBulletTag>(enemy))
            ecb.RemoveComponent<SlowedBySlimeBulletTag>(enemy);
        if (entityManager.HasComponent<DamageEventComponent>(enemy))
            ecb.RemoveComponent<DamageEventComponent>(enemy);

        ecb.AddComponent<Disabled>(enemy);

        inactiveEnemies.Enqueue(enemy);
        enemyCount++;
    }

    public void Initialize()
    {
        waveTimer = firstWaveDelay;
        individualEnemyDelayTimer = 0;
        enemiesToSpawnCounter = enemiesPerWave;
        individualEnemyDelayTimer = 0;
    }
}
