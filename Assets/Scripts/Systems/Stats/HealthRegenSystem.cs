using Unity.Entities;
using UnityEngine;

public partial struct HealthRegenSystem : ISystem
{
    public partial struct SlimeBeamShooterSystem : ISystem
    {
        private EntityManager entityManager;
        private Entity player;
        private float timer;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HealthRegenComponent>();
            state.RequireForUpdate<PlayerHealthComponent>();
            entityManager = state.EntityManager;
            timer = 0f;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!GameManager.Instance.IsPlaying()) return;

            float deltaTime = SystemAPI.Time.DeltaTime;
            timer += deltaTime;

            // Only continue if 0.5 seconds have passed
            if (timer < 0.5f)
                return;

            timer = 0f; // Reset the timer

            PlayerHealthComponent playerHealthComponent;
            if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
            {
                Debug.Log($"Cant Found Player Entity in HealthRegenSystem!");
                return;
            }
            else
                playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);

            // Get Health Regen
            HealthRegenComponent healthRegenComponent;
            int healthRegen = 0;
            if (SystemAPI.HasComponent<HealthRegenComponent>(player))
            {
                healthRegenComponent = entityManager.GetComponentData<HealthRegenComponent>(player);
                healthRegen = healthRegenComponent.healthRegenValue;
            }
            else
            {
                Debug.Log($"Cant Found Health Regen Component in HealthRegenSystem!");
                return;
            }

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);



            // Disable if player is max health
            if (healthRegenComponent.currentLevel <= 0
                || playerHealthComponent.currentHealth == playerHealthComponent.maxHealth 
                || healthRegen <= 0)
                return;


            ecb.AddComponent<HealEventComponent>(player, new HealEventComponent
            {
                healAmount = healthRegen,
            });
        }
    }
}
