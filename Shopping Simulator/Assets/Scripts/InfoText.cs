using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoText : MonoBehaviour
{

    TextMeshProUGUI infoText;

    [TextArea(3, 6)]
    [SerializeField] string turkishText;
  
    [TextArea(3, 6)]
    [SerializeField] string englishText;
   
    [TextArea(3, 6)]
    [SerializeField] string deText; // German
   
    [TextArea(3, 6)]
    [SerializeField] string esText; // Spanish
  
    [TextArea(3, 6)]
    [SerializeField] string itText; // Italian
   
    [TextArea(3, 6)]
    [SerializeField] string frText; // French
  
    [TextArea(3, 6)]
    [SerializeField] string ptText; // Portuguese
    private void Awake()
    {
        infoText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
       
    }

    private void OnEnable()
    {
        string language = PlayerPrefs.GetString("Language", "English");

        switch (language)
        {
            case "English":
                infoText.text = englishText;
                break;
            case "T�rk�e":
                infoText.text = turkishText;
                break;
            case "Deutsch":
                infoText.text = deText; // Alman Dili de�i�keni
                break;
            case "Espa�ol":
                infoText.text = esText; // �spanyol Dili de�i�keni
                break;
            case "Italiano":
                infoText.text = itText; // �talyan Dili de�i�keni
                break;
            case "Fran�ais":
                infoText.text = frText; // Frans�z Dili de�i�keni
                break;
            case "Portugu�s":
                infoText.text = ptText; // Portekiz Dili de�i�keni
                break;
            default:
                infoText.text = englishText;
                break;
        }
    }

}
