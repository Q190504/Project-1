using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    [SerializeField] private int creepPrepare;
    public List<GameObject> SpawnerList;

    private Entity player;
    private EntityManager entityManager;
    private Entity creepPrefab;

    private NativeQueue<Entity> inactiveEnemies;
    private int enemyCount = 0;

    Dictionary<GameObject, int> spawnerQueue = new Dictionary<GameObject, int>();

    [Header("Spawing stats")]
    [SerializeField] private int baseEnemiesPerWave;        // Base number of enemies per wave
    private int enemiesPerWave;                             // Number of enemies per wave
    private int enemiesToSpawnCounter;

    [SerializeField] private float initialSpawnDelay;       // Time before the first wave
    [SerializeField] private float minInterval;             // The lowest possible interval to prevent overwhelming the player
    [SerializeField] private float baseInterval;            // The starting interval between spawns at time 0
    private float waveInterval;                             // Time between waves
    private float waveTimer;
    [SerializeField] private float spawnAcceleration;       // Rate at which the interval decreases per minute
    [SerializeField] private float individualEnemyDelay;    // Delay between spawning each enemy in a wave
    private float individualEnemyDelayTimer;

    private float difficultyMultiplier;
    private double timeSinceStartPlaying;

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
        else
            Debug.LogError("Player entity not found in EnemyManager");

        // Get the baked entity prefab
        EntityQuery enemyPrefabQuery = entityManager.CreateEntityQuery(typeof(EnemyPrefabComponent));
        if (enemyPrefabQuery.CalculateEntityCount() > 0)
        {
            creepPrefab = entityManager.GetComponentData<EnemyPrefabComponent>(enemyPrefabQuery.GetSingletonEntity()).enemyPrefab;
        }
        else
        {
            Debug.LogError("Enemy prefab not found! Make sure it's baked correctly.");
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        Initialize();

        Initialize();
        PrepareEnemy(ecb);

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        #region Spawn Enemies

        if (waveTimer <= 0)
        {
            if (enemiesToSpawnCounter <= 0 && spawnerQueue.Count == 0)
            {
                // Set time till new wave
                waveInterval = Mathf.Max(minInterval, baseInterval - ((float)timeSinceStartPlaying / 60f) * spawnAcceleration);
                waveTimer = waveInterval;

                // Set enemies per wave
                enemiesPerWave = baseEnemiesPerWave + Mathf.FloorToInt((float)timeSinceStartPlaying / 30f);
                enemiesToSpawnCounter = enemiesPerWave;

                spawnerQueue.Clear();

                // Calculate weights based on distance to player
                Dictionary<GameObject, float> spawnerWeights = new Dictionary<GameObject, float>();
                float totalWeight = 0f;

                foreach (var spawner in SpawnerList)
                {
                    float3 playerPos = entityManager.GetComponentData<LocalTransform>(player).Position;
                    float distance = Vector3.Distance(playerPos, spawner.transform.position);
                    float weight = 1f / (distance + 1f); // Closer = heavier
                    spawnerWeights[spawner] = weight;
                    totalWeight += weight;
                }

                // Distribute enemies to spwaners by weight
                int distributedEnemies = 0;
                foreach (var spawner in SpawnerList)
                {
                    float ratio = spawnerWeights[spawner] / totalWeight;
                    int count = Mathf.FloorToInt(ratio * enemiesPerWave);
                    spawnerQueue.Add(spawner, count);
                    distributedEnemies += count;
                }

                // Distribute remainder enemies
                int remaining = enemiesPerWave - distributedEnemies;
                for (int i = 0; i < remaining; i++)
                {
                    var spawner = SpawnerList[i % SpawnerList.Count];
                    spawnerQueue[spawner]++;
                }
            }

            if (individualEnemyDelayTimer <= 0 && spawnerQueue.Count > 0)
            {
                foreach (var spawner in SpawnerList.ToList())
                {
                    if (!spawnerQueue.ContainsKey(spawner)) continue;

                    int toSpawn = spawnerQueue[spawner];
                    if (toSpawn <= 0)
                    {
                        spawnerQueue.Remove(spawner);
                        continue;
                    }

                    EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
                    Vector3 spawnPosition = spawner.transform.position;
                    SpawnEnemy(spawnPosition, ecb);

                    ecb.Playback(entityManager);
                    ecb.Dispose();

                    spawnerQueue[spawner] = toSpawn - 1;

                    if (--enemiesToSpawnCounter <= 0)
                        break;
                }

                individualEnemyDelayTimer = individualEnemyDelay;
            }
            else
            {
                individualEnemyDelayTimer -= Time.deltaTime;
            }
        }
        else
        {
            waveTimer -= Time.deltaTime;
        }

        #endregion
    }

    public void SpawnEnemy(Vector3 position, EntityCommandBuffer ecb)
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        Entity enemyInstance = Take(ecb);

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

        float DifficultyMultiplier = 1 + Mathf.Pow((float)timeSinceStartPlaying / 60f, 1.2f);

        CreepHealthComponent enemyHealthComponent = entityManager.GetComponentData<CreepHealthComponent>(enemyInstance);

        float enemyHP = enemyHealthComponent.baseMaxHealth * DifficultyMultiplier;
        entityManager.SetComponentData(enemyInstance, new CreepHealthComponent
        {
            currentHealth = enemyHP,
            maxHealth = enemyHealthComponent.maxHealth,
        });

        CreepDamageComponent enemyDamageComponent = entityManager.GetComponentData<CreepDamageComponent>(enemyInstance);
        int enemyDamage = (int)(enemyDamageComponent.baseDamage * DifficultyMultiplier);
        entityManager.SetComponentData(enemyInstance, new CreepDamageComponent
        {
            damage = enemyDamage,
            baseDamage = enemyDamageComponent.baseDamage,
        });
    }

    private void PrepareEnemy(EntityCommandBuffer ecb)
    {
        if (creepPrefab == Entity.Null) return;

        for (int i = 0; i < creepPrepare; i++)
        {
            Entity enemy = entityManager.Instantiate(creepPrefab);
            SetEnemyStatus(enemy, false, ecb, entityManager);
            inactiveEnemies.Enqueue(enemy);
            enemyCount++;
        }
    }

    public Entity Take(EntityCommandBuffer ecb)
    {
        if (inactiveEnemies.IsEmpty())
            PrepareEnemy(ecb);

        Entity enemy = inactiveEnemies.Dequeue();
        enemyCount--;
        SetEnemyStatus(enemy, true, ecb, entityManager);
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

        // Return the visual game object
        if (entityManager.HasComponent<VisualReferenceComponent>(enemy))
        {
            VisualReferenceComponent visualReferenceComponent =
                entityManager.GetComponentData<VisualReferenceComponent>(enemy);
            AnimationManager.Instance.ReturnCreep(visualReferenceComponent.gameObject);
        }

        SetEnemyStatus(enemy, false, ecb, entityManager);

        inactiveEnemies.Enqueue(enemy);
        enemyCount++;
    }

    public void Initialize()
    {
        waveTimer = initialSpawnDelay;
        individualEnemyDelayTimer = 0;
        waveInterval = baseInterval;
        enemiesPerWave = baseEnemiesPerWave;
        enemiesToSpawnCounter = 0;
    }

    private void SetEnemyStatus(Entity root, bool status, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        DynamicBuffer<Child> children;

        if (status)
        {
            ecb.RemoveComponent<Disabled>(root);
            if (entityManager.HasComponent<Child>(root))
            {
                children = entityManager.GetBuffer<Child>(root);
                foreach (var child in children)
                {
                    ecb.RemoveComponent<Disabled>(child.Value);
                }
            }

        }
        else
        {
            ecb.AddComponent<Disabled>(root);

            if (entityManager.HasComponent<Child>(root))
            {
                children = entityManager.GetBuffer<Child>(root);
                foreach (var child in children)
                {
                    ecb.AddComponent<Disabled>(child.Value);
                }
            }
        }
    }

    public int GetCreepPrepare()
    {
        return creepPrepare;
    }

    public void SetTimeSinceStartPlaying(double time)
    {
        timeSinceStartPlaying = time;
    }
}
