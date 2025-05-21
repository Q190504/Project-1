using UnityEngine;

public class PlayGameButton : MonoBehaviour
{
    [SerializeField] private VoidPublisherSO setGameStateSO;

    public void OnButtonPressed()
    {
        setGameStateSO.RaiseEvent();
    }
}
