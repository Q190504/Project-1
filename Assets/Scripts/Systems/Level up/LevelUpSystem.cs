using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        foreach (var playerSlots in
                 SystemAPI.Query<RefRO<PlayerUpgradeSlots>>()
                          .WithAll<LevelUpEvent>())
        {
            // Pause game
            GameManager.Instance.TogglePauseGame();

            // Collect all valid upgrade options
            NativeList<UpgradeOption> offerings
                = UpgradeOfferingHelper.GenerateOfferings(playerSlots.ValueRO);

            // 3. Open UI
            GamePlayUIManager.Instance.OpenSelectPanel(offerings);

            offerings.Dispose();
        }
    }
}

