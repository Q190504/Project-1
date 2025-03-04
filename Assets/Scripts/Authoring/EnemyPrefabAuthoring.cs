using Unity.Entities;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;

public class EnemyPrefabAuthoring : MonoBehaviour
{
    public GameObject enemyPrefab;

    public class EnemyPrefabBaker : Baker<EnemyPrefabAuthoring>
    {
        public override void Bake(EnemyPrefabAuthoring authoring)
        {
            Entity enemyEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);
            Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new EnemyPrefabComponent
            {
                enemyPrefab = enemyEntity,
            });
        }
    }
}

public struct EnemyPrefabComponent : IComponentData
{
    public Entity enemyPrefab;
}


