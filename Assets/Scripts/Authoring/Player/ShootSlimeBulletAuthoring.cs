using Unity.Entities;
using UnityEngine;

public class ShootSlimeBulletAuthoring : MonoBehaviour
{
    public float delayTime;

    class ShootSlimeBulletBaker : Baker<ShootSlimeBulletAuthoring>
    {
        public override void Bake(ShootSlimeBulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootSlimeBulletComponent
            {
               delayTime = authoring.delayTime,
            });
        }
    }
}

public struct ShootSlimeBulletComponent : IComponentData
{
    public float delayTime;
}
