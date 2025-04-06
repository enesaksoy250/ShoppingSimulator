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
        string text = LanguageControl.CheckLanguage("<u>Görev #001</u>\r\n<align=left>Hedef Türü:\r\nUzun Hedef Adý\r\n<align=center>0 / 5\r\n<align=left>Toplanan Ödül:", "<u>Mission #001</u>\r\n<align=left>Goal Type:\r\nLong Target Name\r\n<align=center>0 / 5\r\n<align=left>Collect Reward:");     
        missionText.text = text;
    }

   
}
