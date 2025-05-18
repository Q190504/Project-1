using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public partial struct PlayerWorldUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (uiPrefab, entity) in SystemAPI.Query<PlayerWorldUIPrefab>().WithNone<PlayerWorldUI>().WithEntityAccess())
        {
            var newWorldUI = Object.Instantiate(uiPrefab.value.Value);
            ecb.AddComponent(entity, new PlayerWorldUI
            {
                canvasTransform = newWorldUI.transform,
                healthBarSlider = newWorldUI.GetComponentInChildren<Slider>(),
            });
        }

        if (SystemAPI.TryGetSingletonEntity<PlayerHealthComponent>(out Entity player))
        {
            var playerHealth = SystemAPI.GetComponent<PlayerHealthComponent>(player);

            foreach (var (transfrom, worldUI) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW< PlayerWorldUI>>())
            {
                worldUI.ValueRW.canvasTransform.Value.position = transfrom.ValueRO.Position;
                worldUI.ValueRW.healthBarSlider.Value.minValue = 0;
                worldUI.ValueRW.healthBarSlider.Value.maxValue = playerHealth.maxHealth;
                worldUI.ValueRW.healthBarSlider.Value.value = playerHealth.currentHealth;
            }
        }

        foreach (var (worldUI, entity) in SystemAPI.Query<PlayerWorldUI>().WithNone<LocalToWorld>().WithEntityAccess())
        {
            if (worldUI.canvasTransform.Value != null)
            {
                Object.Destroy(worldUI.canvasTransform.Value.gameObject);
            }

            ecb.RemoveComponent<PlayerWorldUI>(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}
