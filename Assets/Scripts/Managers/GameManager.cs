using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private GameObject playerInitalPosition;

    private int totalEnemiesKilled = 0;
    [SerializeField] IntPublisherSO enemiesKilledPublisher;

    private bool hasGameStarted = false;
    [SerializeField] BoolPublisherSO endGamePublisher;

    private double timeSinceStartPlaying = 0;
    [SerializeField] DoublePublisherSO timePublisher;

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
        // TO DO: Remove
        //StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasGameStarted) return;
        AddTime();
    }

    public void AddEnemyKilled()
    {
        totalEnemiesKilled++;
        enemiesKilledPublisher.RaiseEvent(totalEnemiesKilled);
    }

    public void AddTime()
    {
        timeSinceStartPlaying += Time.deltaTime;
        timePublisher.RaiseEvent(timeSinceStartPlaying);
    }

    public void StartGame()
    {
        timeSinceStartPlaying = 0;
        timePublisher.RaiseEvent(timeSinceStartPlaying);

        totalEnemiesKilled = 0;
        enemiesKilledPublisher.RaiseEvent(totalEnemiesKilled);

        hasGameStarted = true;
    }

    public void EndGame(bool result)
    {
        hasGameStarted = false;
        endGamePublisher.RaiseEvent(result);
    }

    public float3 GetPlayerInitialPosition()
    {
        if(playerInitalPosition == null)
        {
            Debug.LogError("Player initial position is not set in GameManager.");
            return float3.zero;
        }

        return playerInitalPosition.gameObject.transform.position;
    }

    public bool GetGameState()
    {
        return hasGameStarted;
    }
}
