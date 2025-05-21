using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "New Float 3 Pulisher", menuName = "Scriptable Objects/Events/Float 3 Publisher")]
public class Float3PublisherSO : ScriptableObject
{
    public UnityAction<float3> OnEventRaised;

    public void RaiseEvent(float3 value)
    {
        OnEventRaised?.Invoke(value);
    }
}
