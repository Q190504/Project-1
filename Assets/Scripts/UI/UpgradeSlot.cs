using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UpgradeSlot : MonoBehaviour
{
    public int ID { get; set; }

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text level;
    [SerializeField] private GameObject levelContainer;

    void Start ()
    {
        ClearSlotInfo();
    }

    public void SetSlotInfo(int ID, Sprite image)
    {
        this.ID = ID;
        this.level.text = "0";
        this.image.sprite = image;
        levelContainer.SetActive(true);
        this.image.enabled = true;
    }

    public void LevelUp()
    {
        int currentLevel = int.Parse(level.text);
        level.text = (currentLevel + 1).ToString();
    }

    public void ClearSlotInfo()
    {
        image.enabled = false;
        levelContainer.SetActive(false);
        ID = -1;
        level.text = "0";
    }
}
