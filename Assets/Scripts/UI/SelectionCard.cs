using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionCard : MonoBehaviour
{
    public UpgradeType upgradeType;
    public TMP_Text level;
    public TMP_Text cardName;
    public Image cardImage;
    public TMP_Text cardDescription;

    public void SetCardInfo(UpgradeType type, string name, string description, Sprite image, int levelNumber)
    {
        upgradeType = type;
        if (levelNumber == 0)
            level.text = "NEW";
        else
            level.text = "Lv: " + levelNumber.ToString();
        cardName.text = name;
        if(image == null)
            cardImage.sprite = image;
        cardDescription.text = description;
    }

    public void OnClick()
    {
        //Apply the selected card type to the player

        GamePlayUIManager.Instance.SetSelectPanel(false);
    }
}
