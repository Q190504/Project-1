using Unity.Entities;
using UnityEngine;

public class CreepMoveSpeedAuthoring : MonoBehaviour
{
    public float speed;

    public class Baker : Baker<CreepMoveSpeedAuthoring>
    {
        public override void Bake(CreepMoveSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CreepMoveSpeedComponent
            {
                speed = authoring.speed,
            });
        }
    }
}

public struct CreepMoveSpeedComponent : IComponentData
{
    public float speed;
}
