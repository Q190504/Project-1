using UnityEngine;

public class OpenSettingPanelButton : MonoBehaviour
{
    [SerializeField] private BoolPublisherSO setSettingPanelSO;

    public void OnButtonPressed()
    {
        setSettingPanelSO.RaiseEvent(true);
    }
}
