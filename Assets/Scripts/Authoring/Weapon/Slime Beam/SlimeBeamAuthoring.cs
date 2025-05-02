using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class SlimeBeamAuthoring : MonoBehaviour
{
    public string weaponId;

    public class Baker : Baker<SlimeBeamAuthoring>
    {
        public override void Bake(SlimeBeamAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponId}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponId} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            SlimeBeamJson weapon = JsonUtility.FromJson<SlimeBeamJson>(jsonText);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SlimeBeamDataBlob>();

            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new SlimeBeamLevelData
                {
                    level = level.level,
                    damage = level.damage,
                    cooldown = level.cooldown,
                    range = level.range,
                    timeBetween = level.timeBetween,
                };
            }

            var blob = builder.CreateBlobAssetReference<SlimeBeamDataBlob>(Allocator.Temp);

            AddComponent(GetEntity(TransformUsageFlags.None), new SlimeBeamComponent
            {
                Data = blob,
                timer = 2f,
                slashCount = 0,
                timeBetween = 0
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class SlimeBeamLevelJson
{
    public int level;
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

[System.Serializable]
public class SlimeBeamJson
{
    public string id;
    public string name;
    public SlimeBeamLevelJson[] levels;
}

public struct SlimeBeamComponent : IComponentData
{
    public float timer;
    public float timeBetween;
    public int slashCount;
    public BlobAssetReference<SlimeBeamDataBlob> Data;
}

public struct SlimeBeamLevelData
{
    public int level;
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

public struct SlimeBeamDataBlob
{
    public BlobString Name;
    public BlobArray<SlimeBeamLevelData> Levels;
}