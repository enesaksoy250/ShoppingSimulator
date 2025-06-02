using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionText : MonoBehaviour
{
     
    private TextMeshProUGUI missionText;

    private void Awake()
    {
        missionText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        string text = LanguageManager.instance.GetLocalizedValue("MissionObjectiveDisplay").Replace("\\n", "\n");     
        missionText.text = text;
    }

   
}
