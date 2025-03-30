using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class WeaponDatabaseAuthoring : MonoBehaviour
{
    [Header("Weapons List")]
    public WeaponStats[] weapons;

    class Baker : Baker<WeaponDatabaseAuthoring>
    {
        public override void Bake(WeaponDatabaseAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<WeaponDatabase>();

            // Allocate weapons array
            var weapons = builder.Allocate(ref root.weapons, authoring.weapons.Length);
            for (int i = 0; i < authoring.weapons.Length; i++)
            {
                weapons[i].type = authoring.weapons[i].type;

                // Allocate levels array (6 levels per weapon)
                var levels = builder.Allocate(ref weapons[i].levels, 6);
                for (int j = 0; j < 6; j++)
                {
                    levels[j].damage = authoring.weapons[i].levels[j].damage;
                    levels[j].cooldownTime = authoring.weapons[i].levels[j].cooldownTime;
                    levels[j].range = authoring.weapons[i].levels[j].range;
                }
            }

            // Create Blob Asset Reference
            var blobReference = builder.CreateBlobAssetReference<WeaponDatabase>(Allocator.Persistent);

            builder.Dispose();

            // Register the Blob Asset to the Baker for de-duplication and reverting.
            AddBlobAsset<WeaponDatabase>(ref blobReference, out var hash);

            // Create Entity and attach WeaponDatabaseComponent
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new WeaponDatabaseComponent { weaponDatabase = blobReference });
        }
    }
}
