using UnityEngine;

public class OpenConfirmExitGamePanelButton : MonoBehaviour
{
    [SerializeField] private BoolPublisherSO openConfirmExitGamePanel;

    public void OnButtonPressed()
    {
        openConfirmExitGamePanel.RaiseEvent(true);
    }
}