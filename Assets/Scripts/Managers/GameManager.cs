using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public enum GameState
{
    NotStarted,
    Initializing,
    Upgrading,
    Playing,
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private bool needToReset;

    [SerializeField] private GameObject playerInitalPosition;

    private int totalEnemiesKilled = 0;
    [SerializeField] private IntPublisherSO enemiesKilledPublisher;

    private GameState gameState;
    [SerializeField] private BoolPublisherSO endGamePublisher;

    private double timeSinceStartPlaying = 0;
    [SerializeField] private DoublePublisherSO timePublisher;

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
        SetNeedToReset(true);
        SetGameState(GameState.NotStarted);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying())
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

        // Win if reach 15 minutes
        if ((int)timeSinceStartPlaying / 60 == 15 && (int)timeSinceStartPlaying % 60 == 0)
        {
            EndGame(true);
        }
    }

    public void StartGame()
    {
        timeSinceStartPlaying = 0;
        timePublisher.RaiseEvent(timeSinceStartPlaying);

        totalEnemiesKilled = 0;
        enemiesKilledPublisher.RaiseEvent(totalEnemiesKilled);

        SetNeedToReset(true);
        SetGameState(GameState.Initializing);
    }

    public void EndGame(bool result)
    {
        SetGameState(GameState.NotStarted);
        endGamePublisher.RaiseEvent(result);
    }

    public void ExitGameMode()
    {
        SetGameState(GameState.NotStarted);
    }

    public float3 GetPlayerInitialPosition()
    {
        if (playerInitalPosition == null)
        {
            Debug.LogError("Player initial position is not set in GameManager.");
            return float3.zero;
        }

        return playerInitalPosition.gameObject.transform.position;
    }

    public bool IsPlaying()
    {
        return gameState == GameState.Playing;
    }

    public bool IsUpgrading()
    {
        return gameState == GameState.Upgrading;
    }

    public bool IsNotStarted()
    {
        return gameState == GameState.NotStarted;
    }

    public bool IsInitializing()
    {
        return gameState == GameState.Initializing;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    public bool GetNeedToReset()
    {
        return needToReset;
    }

    public void SetNeedToReset(bool value)
    {
        needToReset = value;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void TogglePauseGameForUpgrading()
    {
        if (gameState == GameState.Playing)
        {
            SetGameState(GameState.Upgrading);
            //Time.timeScale = 0f; // Pause the game
        }
        else if (gameState == GameState.Upgrading)
        {
            SetGameState(GameState.Playing);
            //Time.timeScale = 1f; // Resume the game
        }
    }
}
