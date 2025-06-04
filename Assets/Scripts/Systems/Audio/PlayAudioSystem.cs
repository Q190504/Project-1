using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct PlayAudioSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (sfxEvent, entity) in SystemAPI.Query<RefRO<PlaySFXEvent>>().WithEntityAccess())
        {
            AudioManager.Instance.PlaySFX(sfxEvent.ValueRO.sfxId);

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}