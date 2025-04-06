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
        string text = LanguageControl.CheckLanguage("<u>G�rev #001</u>\r\n<align=left>Hedef T�r�:\r\nUzun Hedef Ad�\r\n<align=center>0 / 5\r\n<align=left>Toplanan �d�l:", "<u>Mission #001</u>\r\n<align=left>Goal Type:\r\nLong Target Name\r\n<align=center>0 / 5\r\n<align=left>Collect Reward:");     
        missionText.text = text;
    }

   
}
