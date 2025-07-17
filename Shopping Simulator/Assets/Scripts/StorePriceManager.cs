using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorePriceManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI[] priceTexts;

    private string countryCode;

    private readonly List<Dictionary<string, string>> productPrices = new()
    {
        new Dictionary<string, string>  // 0. ürün
        {
            { "TR", "19.99 TL" }, { "US", "$1.00" }, { "EC", "$1.00" }, { "DEFAULT", "€1.00" }
        },
        new Dictionary<string, string>  // 1. ürün
        {
            { "TR", "24.99 TL" }, { "US", "$1.00" }, { "EC", "$1.00" }, { "DEFAULT", "€1.00" }
        },
        new Dictionary<string, string>  // 2. ürün
        {
            { "TR", "44.99 TL" }, { "US", "$1.50" }, { "EC", "$1.50" }, { "DEFAULT", "€1.50" }
        },
        new Dictionary<string, string>  // 3. ürün
        {
            { "TR", "99.99 TL" }, { "US", "$3.00" }, { "EC", "$3.00" }, { "DEFAULT", "€3.00" }
        },
        new Dictionary<string, string>  // 4. ürün
        {
            { "TR", "174.99 TL" }, { "US", "$5.00" }, { "EC", "$5.00" }, { "DEFAULT", "€5.00" }
        }
    };

    private int number;

    private void Start()
    {
        countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        number = 0;
    }

    public void SetPrice()
    {
        foreach (var price in priceTexts)
        {
            price.text = GetPriceByIndex(number++);
        }

        number = 0;

        CheckRemoveAdState();
    }

    private string GetPriceByIndex(int index)
    {
        if (index < 0 || index >= productPrices.Count)
            return "N/A";

        var priceDict = productPrices[index];
        return priceDict.ContainsKey(countryCode) ? priceDict[countryCode] : priceDict["DEFAULT"];
    }

    public static void CheckRemoveAdState()
    {
        if (PlayerPrefs.GetInt("RemoveAd") == 1)
        {
            UIManager.Instance.RemoveAdPanel.GetComponent<Button>().interactable = false;
            string language = LanguageManager.GetLanguage();
            UIManager.Instance.StorePriceText.text = LanguageManager.instance.GetLocalizedValue("PurchasedText");
        }
    }

}
