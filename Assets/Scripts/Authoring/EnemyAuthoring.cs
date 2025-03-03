using Unity.Entities;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;

public class EnemyAuthoring : MonoBehaviour
{
    public GameObject enemyPrefab;
}

public class EnemyBaker : Baker<EnemyAuthoring>
{
    public override void Bake(EnemyAuthoring authoring)
    {
        Entity enemyEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);
        Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

        AddComponent(mainEntity, new EnemyPrefabComponent
        {
            enemyPrefab = enemyEntity,
        });
    }
}

public struct EnemyPrefabComponent : IComponentData
{
    public Entity enemyPrefab;
}


