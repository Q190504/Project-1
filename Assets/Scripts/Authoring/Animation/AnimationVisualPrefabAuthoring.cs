using Unity.Entities;
using UnityEngine;

public class AnimationVisualPrefabAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject creepPrefab;
    [SerializeField] private GameObject slimeBulletSlowZonePrefab;
    [SerializeField] private GameObject slimeBeamPrefab;
    [SerializeField] private GameObject radiantFieldPrefab;
    [SerializeField] private GameObject poisonCloud;
    [SerializeField] private GameObject hitEffectPrefab;

    class Baker : Baker<AnimationVisualPrefabAuthoring>
    {
        public override void Bake(AnimationVisualPrefabAuthoring authoring)
        {
            Entity playerPrefabEntity = GetEntity(TransformUsageFlags.None);

            AddComponentObject(playerPrefabEntity, new AnimationVisualPrefabsComponent
            {
                player = authoring.playerPrefab,
                creep = authoring.creepPrefab,
                slimeBeam = authoring.slimeBeamPrefab,
                slimeBulletSlowZone = authoring.slimeBulletSlowZonePrefab,
                hitEffect = authoring.hitEffectPrefab,
                poisonCloud = authoring.poisonCloud,
                radiantField = authoring.radiantFieldPrefab,
            });
        }
    }
}

public class AnimationVisualPrefabsComponent : IComponentData
{
    public GameObject player;
    public GameObject creep;   
    public GameObject slimeBulletSlowZone;   
    public GameObject slimeBeam;   
    public GameObject radiantField;   
    public GameObject poisonCloud;   
    public GameObject hitEffect;
}
