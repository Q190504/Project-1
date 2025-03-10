using Unity.Entities;
using UnityEngine;

public class EnemyTargetAuthoring : MonoBehaviour
{
    public GameObject targetGameObject;

    public class EnemyTargetBaker : Baker<EnemyTargetAuthoring>
    {
        public override void Bake(EnemyTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyTargetComponent
            {
                targetEntity = GetEntity(authoring.targetGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct EnemyTargetComponent : IComponentData
{
    public Entity targetEntity;
}

