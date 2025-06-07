using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
public partial struct CollectExperienceOrbSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerLevelComponent>();
        state.RequireForUpdate<ExperienceOrbComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        NativeList<Entity> orbsToReturn = new NativeList<Entity>(Allocator.TempJob);

        var job = new PlayerSuckExperienceOrbJob
        {
            experienceOrbLookup = SystemAPI.GetComponentLookup<ExperienceOrbComponent>(),
            playerLevelLookup = SystemAPI.GetComponentLookup<PlayerLevelComponent>(),
            ecb = ecb,
            orbsToReturn = orbsToReturn,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        foreach (var orb in orbsToReturn)
        {
            ExperienceOrbManager.Instance.Return(orb, ecb);
        }

        orbsToReturn.Dispose();
    }
}

[BurstCompile]
struct PlayerSuckExperienceOrbJob : ITriggerEventsJob
{
    public ComponentLookup<ExperienceOrbComponent> experienceOrbLookup;
    public ComponentLookup<PlayerLevelComponent> playerLevelLookup;
    public EntityCommandBuffer ecb;
    public NativeList<Entity> orbsToReturn;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsPlayer = playerLevelLookup.HasComponent(entityA);
        bool entityBIsPlayer = playerLevelLookup.HasComponent(entityB);

        if (entityAIsPlayer || entityBIsPlayer)
        {
            Entity orbEntity = entityAIsPlayer ? entityB : entityA;
            Entity playerEntity = entityAIsPlayer ? entityA : entityB;

            if (experienceOrbLookup.HasComponent(orbEntity))
            {
                var experienceOrbComponent = experienceOrbLookup[orbEntity];

                if (!experienceOrbComponent.hasBeenCollected)
                {
                    //Plus XP
                    PlusXP(playerEntity, experienceOrbComponent.experience);

                    //Set hasBeenCollected = true
                    ecb.SetComponent(orbEntity, new ExperienceOrbComponent
                    {
                        hasBeenCollected = true,
                        isBeingPulled = false,
                        experience = experienceOrbComponent.experience,
                    });

                    // Add orb to return list
                    orbsToReturn.Add(orbEntity);
                }
            }
        }
    }

    private void PlusXP(Entity playerEntity, int healAmount)
    {
        ecb.AddComponent(playerEntity, new AddExperienceComponent { experienceAmount = healAmount });
    }
}