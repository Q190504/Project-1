using Unity.Entities;
using UnityEngine;

public class EnemyTagAuthoring : MonoBehaviour
{
    public int damage;
    public float speed;

    public class EnemyTagBaker : Baker<EnemyTagAuthoring>
    {
        public override void Bake(EnemyTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyTagComponent
            {
                isStunned = false,
                damage = authoring.damage,
                speed = authoring.speed,
            });
        }
    }
}

public struct EnemyTagComponent : IComponentData
{
    public bool isStunned;
    public int damage;
    public float speed;
}
