using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

public class Float3EventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<float3> EventResponse;
    [SerializeField] private Float3PublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(float3 value)
    {
        EventResponse?.Invoke(value);
    }
}
