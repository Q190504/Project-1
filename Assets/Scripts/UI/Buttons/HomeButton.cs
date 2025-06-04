using UnityEngine;

public class HomeButton : BaseButton
{
    [SerializeField] private VoidPublisherSO setGameStateSO;

    public void OnButtonPressed()
    {
        PlayClickSound();
        setGameStateSO.RaiseEvent();
    }
}
