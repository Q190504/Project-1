using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UpgradeSlot : MonoBehaviour
{
    public int ID { get; set; }

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject levelContainer;

    void Start ()
    {
        ClearSlotInfo();
    }

    public void SetSlotInfo(int ID, Sprite image, int level)
    {
        this.ID = ID;
        this.levelText.text = level.ToString();
        this.image.sprite = image;
        levelContainer.SetActive(true);
        this.image.enabled = true;
    }

    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }

    public void ClearSlotInfo()
    {
        image.enabled = false;
        levelContainer.SetActive(false);
        ID = -1;
        levelText.text = "0";
    }
}
