using TMPro;
using UnityEngine;

public class LuckyCharmInfoView : MonoBehaviour
{
    public RectTransform PriceContainer;
    public TextMeshProUGUI PriceText;
    public TextMeshProUGUI SoldOutText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;

    public RectTransform RectTransform => transform as RectTransform;

    public void Hydrate(LuckyCharmDefData data, bool isPurchased)
    {
        PriceText.text = data.price.ToString();
        PriceContainer.gameObject.SetActive(!isPurchased);

        NameText.text = data.name;
        DescriptionText.text = data.effect;

        SoldOutText.gameObject.SetActive(isPurchased);
    }

    public void SetHover(bool enable)
    {
        NameText.gameObject.SetActive(enable);
        DescriptionText.gameObject.SetActive(enable);
    }
}