using Unity.Entities;
using UnityEngine;

public class ArmorAuthoring : MonoBehaviour
{
    public int armorVaule;

    public int currentLevel;
    public int maxLevel;
    public int increment;

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
    public int armorVaule;

    public int currentLevel;
    public int maxLevel;
    public int increment;
}