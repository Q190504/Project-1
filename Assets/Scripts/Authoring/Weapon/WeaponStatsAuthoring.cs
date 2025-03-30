using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[System.Serializable]
public struct WeaponLevelStats
{
    public float damage;
    public float range;
    public float cooldownTime;
}

public class WeaponStatsAuthoring : MonoBehaviour
{
    public WeaponType Type;
    public WeaponLevelStats[] LevelStats = new WeaponLevelStats[5]; 

    class Baker : Baker<WeaponStatsAuthoring>
    {
        public override void Bake(WeaponStatsAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<WeaponStatsBlob>();

            root.Type = authoring.Type;

            var levels = builder.Allocate(ref root.Levels, 5);
            for (int i = 0; i < 5; i++)
            {
                levels[i].damage = authoring.LevelStats[i].damage;
                levels[i].range = authoring.LevelStats[i].range;
                levels[i].cooldownTime = authoring.LevelStats[i].cooldownTime;
            }

            var blobAsset = builder.CreateBlobAssetReference<WeaponStatsBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddBlobAsset(ref blobAsset, out var hash);
        }
    }
}
