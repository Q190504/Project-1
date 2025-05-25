using UnityEngine;

public class CloseConfirmExitGamePanel : MonoBehaviour
{
    [SerializeField] private BoolPublisherSO closeConfirmExitGamePanel;

    public void OnButtonPressed()
    {
        closeConfirmExitGamePanel.RaiseEvent(false);
    }
}
