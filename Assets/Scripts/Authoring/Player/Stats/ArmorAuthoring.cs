using Unity.Entities;
using UnityEngine;

public class ArmorAuthoring : MonoBehaviour
{
    public float armorVaule;

    public int currentLevel;
    public int maxLevel;
    public float increment;

    public class Baker : Baker<ArmorAuthoring>
    {
        public override void Bake(ArmorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ArmorComponent
            {
                armorVaule = authoring.armorVaule,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct ArmorComponent : IComponentData
{
    public float armorVaule;

    public int currentLevel;
    public int maxLevel;
    public float increment;
}