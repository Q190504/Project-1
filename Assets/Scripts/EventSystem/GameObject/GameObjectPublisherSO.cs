using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Bullet Pulisher", menuName = "Scriptable Objects/Events/Bullet Publisher")]
public class GameObjectPublisherSO : ScriptableObject
{
    public UnityAction<GameObject, string, int> OnEventRaised;

    public void RaiseEvent(GameObject obj, string tag="", int name=-1)
    {
        OnEventRaised?.Invoke(obj, tag, name);
    }
}