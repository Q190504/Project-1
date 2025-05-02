using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class SlimeBeamShooterAuthoring : MonoBehaviour
{
    public string weaponId;

    public class Baker : Baker<SlimeBeamShooterAuthoring>
    {
        public override void Bake(SlimeBeamShooterAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponId}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponId} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            SlimeBeamShooterJson weapon = JsonUtility.FromJson<SlimeBeamShooterJson>(jsonText);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SlimeBeamShooterDataBlob>();

            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new SlimeBeamShooterLevelData
                {
                    level = level.level,
                    damage = level.damage,
                    cooldown = level.cooldown,
                    range = level.range,
                    timeBetween = level.timeBetween,
                };
            }

            var blob = builder.CreateBlobAssetReference<SlimeBeamShooterDataBlob>(Allocator.Temp);

            AddComponent(GetEntity(TransformUsageFlags.None), new SlimeBeamShooterComponent
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
public class SlimeBeamShooterLevelJson
{
    public int level;
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

[System.Serializable]
public class SlimeBeamShooterJson
{
    public string id;
    public string name;
    public SlimeBeamShooterLevelJson[] levels;
}

public struct SlimeBeamShooterComponent : IComponentData
{
    public float timer;
    public float timeBetween;
    public int slashCount;
    public BlobAssetReference<SlimeBeamShooterDataBlob> Data;
}

public struct SlimeBeamShooterLevelData
{
    public int level;
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

public struct SlimeBeamShooterDataBlob
{
    public BlobString Name;
    public BlobArray<SlimeBeamShooterLevelData> Levels;
}