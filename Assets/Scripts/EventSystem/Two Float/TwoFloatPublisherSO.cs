using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "New Two Float Pulisher", menuName = "Scriptable Objects/Events/Two Float Publisher")]
public class TwoFloatPublisherSO : ScriptableObject
{
    public UnityAction<float, float> OnEventRaised;

    public void RaiseEvent(float value1, float value2)
    {
        OnEventRaised?.Invoke(value1, value2);
    }
}
