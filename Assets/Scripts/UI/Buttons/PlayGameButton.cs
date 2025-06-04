using UnityEngine;

public class PlayGameButton : BaseButton
{
    [SerializeField] private VoidPublisherSO setGameStateSO;

    public void OnButtonPressed()
    {
        PlayClickSound();   
        setGameStateSO.RaiseEvent();
    }
}
