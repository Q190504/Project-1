using Unity.Entities;
using UnityEngine;

public class ImpassibleTagAuthoring : MonoBehaviour
{
    class Baker : Baker<ImpassibleTagAuthoring>
    {
        public override void Bake(ImpassibleTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ImpassibleTagComponent());
        }
    }
}

public struct ImpassibleTagComponent : IComponentData 
{

}
