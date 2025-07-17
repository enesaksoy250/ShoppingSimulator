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
            case "Türkçe":
                infoText.text = turkishText;
                break;
            case "Deutsch":
                infoText.text = deText; // Alman Dili deðiþkeni
                break;
            case "Español":
                infoText.text = esText; // Ýspanyol Dili deðiþkeni
                break;
            case "Italiano":
                infoText.text = itText; // Ýtalyan Dili deðiþkeni
                break;
            case "Français":
                infoText.text = frText; // Fransýz Dili deðiþkeni
                break;
            case "Português":
                infoText.text = ptText; // Portekiz Dili deðiþkeni
                break;
            default:
                infoText.text = englishText;
                break;
        }
    }

}
