using Unity.Entities;
using UnityEngine;

public class EnemyTagAuthoring : MonoBehaviour
{

    public class EnemyTagBaker : Baker<EnemyTagAuthoring>
    {
        public override void Bake(EnemyTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyComponentTag());
        }
    }
}

public struct EnemyComponentTag : IComponentData
{

}
