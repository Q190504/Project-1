using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerWorldUI : ICleanupComponentData
{
    public UnityObjectRef<Transform> canvasTransform;
    public UnityObjectRef<Slider> healthBarSlider;
    public UnityObjectRef<TMP_Text> levelText;
}

public struct PlayerWorldUIPrefab : IComponentData
{
    public UnityObjectRef<GameObject> value;
}
