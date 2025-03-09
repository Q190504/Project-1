using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool IsPlayerAround {  get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsPlayerAround = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IsPlayerAround = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IsPlayerAround = false;
    }
}
