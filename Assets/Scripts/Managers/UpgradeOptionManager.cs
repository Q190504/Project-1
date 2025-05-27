using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UpgradeOptionManager : MonoBehaviour
{
    private static UpgradeOptionManager _instance;

    private float timer;
    [SerializeField] private float totalTime;
    private List<UpgradeCard> upgradeOptions;

    [SerializeField] private TwoFloatPublisherSO updateCountdownSO;

    public static UpgradeOptionManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<UpgradeOptionManager>();
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
        timer = totalTime;
        upgradeOptions = new List<UpgradeCard>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsUpgrading())
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                // select the first upgrade option
                ChooseFirstOption();

                timer = totalTime;
                GameManager.Instance.TogglePauseGame();
            }

            updateCountdownSO.RaiseEvent(timer, totalTime);
        }
    }

    private void ChooseFirstOption()
    {
       if (upgradeOptions != null && upgradeOptions.Count > 0)
        {
            // Select the first upgrade option
            UpgradeCard firstOption = upgradeOptions[0];
            firstOption.Select();
        }
    }

    public void ClearOption()
    {
        upgradeOptions.Clear();
    }

    public void SetTimer()
    {
        timer = totalTime;
    }

    public void AddUpgradeOption(GameObject card)
    {
        UpgradeCard option = card.GetComponent<UpgradeCard>();

        if (option != null && !upgradeOptions.Contains(option))
        {
            upgradeOptions.Add(option);
        }
    }
}
