using UnityEngine;
using UnityEngine.Events;

public class TwoFloatEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<float, float> EventResponse;
    [SerializeField] private TwoFloatPublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(float value1, float value2)
    {
        EventResponse?.Invoke(value1, value2);
    }
}
