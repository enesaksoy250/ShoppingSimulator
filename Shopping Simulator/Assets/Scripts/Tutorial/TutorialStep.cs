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
                case "T�rk�e":
                    return descriptionTR;
                case "Deutsch":
                    return descriptionDE;
                case "Espa�ol":
                    return descriptionES;
                case "Italiano":
                    return descriptionIT;
                case "Fran�ais":
                    return descriptionFR;
                case "Portugu�s":
                    return descriptionPT;
                default:
                    // E�er kay�tl� dil beklenmedik bir de�erse, varsay�lan olarak �ngilizce'yi d�nd�r
                    return descriptionEN;
            }
        }
    }

}
