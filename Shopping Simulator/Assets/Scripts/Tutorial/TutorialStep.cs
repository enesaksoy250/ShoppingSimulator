using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]   
public class TutorialStep
{
    [Header("Localization")]
    [TextArea(2, 5)] public string descriptionTR;
    [TextArea(2, 5)] public string descriptionEN;
    [TextArea(2, 5)] public string descriptionDE; // German
    [TextArea(2, 5)] public string descriptionES; // Spanish
    [TextArea(2, 5)] public string descriptionIT; // Italian
    [TextArea(2, 5)] public string descriptionFR; // French
    [TextArea(2, 5)] public string descriptionPT; // Portuguese

    [Header("Step Settings")]
    public Transform targetTransform;
    public Vector3 cameraOffset;
    public Vector2 infoPanelPosition;
    //public GameObject arrowPrefab;
    public UnityEvent onStepEnter;


    public string Description
    {
        get
        {
           
            string currentLanguage = PlayerPrefs.GetString("Language", "English");

            switch (currentLanguage)
            {
                case "English":
                    return descriptionEN;
                case "Türkçe":
                    return descriptionTR;
                case "Deutsch":
                    return descriptionDE;
                case "Español":
                    return descriptionES;
                case "Italiano":
                    return descriptionIT;
                case "Français":
                    return descriptionFR;
                case "Português":
                    return descriptionPT;
                default:
                    // Eðer kayýtlý dil beklenmedik bir deðerse, varsayýlan olarak Ýngilizce'yi döndür
                    return descriptionEN;
            }
        }
    }

}
