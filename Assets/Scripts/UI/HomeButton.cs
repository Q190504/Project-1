using UnityEngine;

public class HomeButton : MonoBehaviour
{
    [SerializeField] private VoidPublisherSO setGameStateSO;

    public void OnButtonPressed()
    {
        setGameStateSO.RaiseEvent();
    }
}
