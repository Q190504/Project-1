using Unity.Entities;
using UnityEngine;

public class CreepTagAuthoring : MonoBehaviour
{
    public class CreepTagBaker : Baker<CreepTagAuthoring>
    {
        public override void Bake(CreepTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyTagComponent());
        }
    }
}

public struct CreepTagComponent : IComponentData
{

}
