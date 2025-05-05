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
            return PlayerPrefs.GetString("Language", "Turkish") == "English" ? descriptionEN : descriptionTR;
        }
    }

}
