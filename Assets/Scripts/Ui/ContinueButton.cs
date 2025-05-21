using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private BoolPublisherSO setSettingPanelSO;

    public void OnButtonPressed()
    {
        setSettingPanelSO.RaiseEvent(false);
    }
}
