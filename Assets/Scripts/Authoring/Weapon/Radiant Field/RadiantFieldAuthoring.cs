using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class RadiantFieldAuthoring : MonoBehaviour
{
    public WeaponType weaponId = WeaponType.RadiantField;

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

            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`.
            ref var root = ref builder.ConstructRoot<RadiantFieldDataBlob>();

            // Now fill the constructed root with the data:
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

            // Now copy the data from the builder into its final place, which will
            // use the persistent allocator
            var blobReference = builder.CreateBlobAssetReference<RadiantFieldDataBlob>(Allocator.Persistent);

            // Make sure to dispose the builder itself so all internal memory is disposed.
            builder.Dispose();

            // Register the Blob Asset to the Baker for de-duplication and reverting.
            AddBlobAsset<RadiantFieldDataBlob>(ref blobReference, out var hash);

            Entity radiantFieldEntity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(radiantFieldEntity, new RadiantFieldComponent
            {
                Data = blobReference,
                timer = 0f,
                timeBetween = weapon.timeBetween,
                currentLevel = 0,
                previousLevel = -1,
                lastTickTime = 0,
            });

            //using var builder = new BlobBuilder(Allocator.Temp);
            //{
            //    ref var root = ref builder.ConstructRoot<RadiantFieldDataBlob>();

            //    var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            //    for (int i = 0; i < weapon.levels.Length; i++)
            //    {
            //        var level = weapon.levels[i];
            //        levels[i] = new RadiantFieldLevelData
            //        {
            //            damagePerTick = level.damagePerTick,
            //            cooldown = level.cooldown,
            //            radius = level.radius,
            //            slowModifier = level.slowModifier,
            //        };
            //    }

            //    var blob = builder.CreateBlobAssetReference<RadiantFieldDataBlob>(Allocator.Temp);

            //    Entity radiantFieldEntity = GetEntity(TransformUsageFlags.Dynamic);

            //    AddComponent(radiantFieldEntity, new RadiantFieldComponent
            //    {
            //        Data = blob,
            //        timer = 2f,
            //        timeBetween = weapon.timeBetween,
            //        currentLevel = 0,
            //        previousLevel = -1,
            //        lastTickTime = 0,
            //    });
            //}
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