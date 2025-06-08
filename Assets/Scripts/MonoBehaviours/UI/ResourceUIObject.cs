using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUIObject : MonoBehaviour
{
    public ResourceType resourceType;
    public Image resourceImage;
    public Image backgroundImage;
    public TMP_Text resourceAmountText;

    public void Setup(ResourceTypeSO resourceTypeSO)
    {
        this.resourceType = resourceTypeSO.ResourceType;
        resourceImage.sprite = resourceTypeSO.Sprite;
        resourceAmountText.text = "0";

        switch (resourceType)
        {
            case ResourceType.Iron:
                backgroundImage.color = new Color(0.34f, 0.34f, 0.34f);
                break;
            case ResourceType.Gold:
                backgroundImage.color = new Color(0.56f, 0.45f, 0f);
                break;
            case ResourceType.Oil:
                backgroundImage.color = new Color(0f, 0.45f, 0.56f);
                break;
        }
    }


    public void UpdateResourceAmount(int amount)
    {
        resourceAmountText.text = amount.ToString();
    }
}
