using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    //[Range(0f, 1f)]
    //public float SKILL_1_THRESHOLD;
    //[Range(0f, 1f)]
    //public float SKILL_2_THRESHOLD;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
