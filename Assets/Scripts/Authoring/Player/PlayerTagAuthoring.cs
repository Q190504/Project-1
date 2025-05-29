using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using System.IO;
using Unity.Mathematics;

public class PlayerTagAuthoring : MonoBehaviour
{
    public GameObject worldUIPrefab;

    class PlayerTagBaker : Baker<PlayerTagAuthoring>
    {
        public override void Bake(PlayerTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerTagComponent());

            AddComponent(entity, new PlayerWorldUIPrefab
            {
                value = authoring.worldUIPrefab,
            });
        }
    }
}

public struct PlayerTagComponent : IComponentData 
{
    public bool isStunned;
}
