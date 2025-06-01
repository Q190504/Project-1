using Unity.Entities;
using UnityEngine;

public class CreepTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<CreepTagAuthoring>
    {
        public override void Bake(CreepTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CreepTagComponent());
        }
    }
}

public struct CreepTagComponent : IComponentData
{

}
