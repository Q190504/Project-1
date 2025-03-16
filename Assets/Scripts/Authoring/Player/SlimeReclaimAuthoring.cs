using Unity.Entities;
using UnityEngine;

public class SlimeReclaimAuthoring : MonoBehaviour
{
    public float cooldownTime;
    public float stunPlayerTime;
    public float hpHealPrecentPerBullet;
    public float bulletSpeedWhenSummoned;

    class SlimeReclaimBulletBaker : Baker<SlimeReclaimAuthoring>
    {
        public override void Bake(SlimeReclaimAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SlimeReclaimComponent
            {
                cooldownTime = authoring.cooldownTime,
                stunPlayerTime = authoring.stunPlayerTime,
                hpHealPrecentPerBullet = authoring.hpHealPrecentPerBullet,
                bulletSpeedWhenSummoned = authoring.bulletSpeedWhenSummoned,
            });
        }
    }
}

public struct SlimeReclaimComponent : IComponentData
{
    public float cooldownTime;
    public float stunPlayerTime;
    public float hpHealPrecentPerBullet;
    public float bulletSpeedWhenSummoned;
}
