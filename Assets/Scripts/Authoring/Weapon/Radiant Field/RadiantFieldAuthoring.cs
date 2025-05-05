using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class RadiantFieldAuthoring : MonoBehaviour
{
    public string weaponId;

    public class Baker : Baker<RadiantFieldAuthoring>
    {
        public override void Bake(RadiantFieldAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponId}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponId} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            RadiantFieldJson weapon = JsonUtility.FromJson<RadiantFieldJson>(jsonText);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<RadiantFieldDataBlob>();

            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];
                levels[i] = new RadiantFieldLevelData
                {
                    damagePerTick = level.damagePerTick,
                    cooldown = level.cooldown,
                    radius = level.radius,
                    slowModifier = level.slowModifier,
                };
            }

            var blob = builder.CreateBlobAssetReference<RadiantFieldDataBlob>(Allocator.Temp);

            Entity radiantFieldEntity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(radiantFieldEntity, new RadiantFieldComponent
            {
                Data = blob,
                timer = 2f,
                timeBetween = weapon.timeBetween,
                currentLevel = 1, //TO DO: set to 0
                previousLevel = 0, //TO DO: set to -1
                lastTickTime = 0,
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class RadiantFieldLevelJson
{
    public int damagePerTick;
    public float cooldown;
    public float radius;
    public float slowModifier;
}

[System.Serializable]
public class RadiantFieldJson
{
    public string id;
    public string name;
    public float timeBetween;
    public RadiantFieldLevelJson[] levels;
}

public struct RadiantFieldLevelData
{
    public int damagePerTick;
    public float cooldown;
    public float radius;
    public float slowModifier;
}

public struct RadiantFieldDataBlob
{
    public BlobString Name;
    public BlobArray<RadiantFieldLevelData> Levels;
}

public struct RadiantFieldComponent : IComponentData
{
    public int currentLevel;
    public int previousLevel;
    public float timer;
    public float timeBetween;
    public double lastTickTime;
    public BlobAssetReference<RadiantFieldDataBlob> Data;
}