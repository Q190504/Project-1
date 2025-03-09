using UnityEngine;

public class PlayerHealth : BaseHealth
{
    [SerializeField] protected FloatPublisherSO onStart;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onStart.RaiseEvent(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
