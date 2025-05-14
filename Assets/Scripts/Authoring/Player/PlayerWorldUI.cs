using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerWorldUI : ICleanupComponentData
{
    public UnityObjectRef<Transform> canvasTransform;
    public UnityObjectRef<Slider> healthBarSlider;
}

public struct PlayerWorldUIPrefab : IComponentData
{
    public UnityObjectRef<GameObject> value;
}
