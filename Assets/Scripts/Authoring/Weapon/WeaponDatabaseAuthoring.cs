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

                    //Slime Bullet
                    levels[j].maximumDistanceBetweenBullets = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].maximumDistanceBetweenBullets : 0f;
                    levels[j].minimumDistanceBetweenBullets = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].minimumDistanceBetweenBullets : 0f;
                    levels[j].bulletCount = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].bulletCount : 0;
                    levels[j].passthroughDamageModifier = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].passthroughDamageModifier : 0f;
                    levels[j].moveSpeed = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].moveSpeed : 0f;
                    levels[j].distance = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].distance : 0f;
                    levels[j].existDuration = weapons[i].type == WeaponType.SlimeBullet ? authoring.weapons[i].levels[j].existDuration : 0f;

                    //Slime Slash
                    levels[j].radius = weapons[i].type == WeaponType.SlimeSlash ? authoring.weapons[i].levels[j].radius : 0f;
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
