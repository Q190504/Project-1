using UnityEngine;

public class ConfirmExitGameButton : MonoBehaviour
{
    [SerializeField] private VoidPublisherSO exitGameSO;

    public void OnButtonPressed()
    {
        exitGameSO.RaiseEvent();
    }
}
