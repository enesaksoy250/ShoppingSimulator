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

    private void Awake()
    {
        infoText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        string language = PlayerPrefs.GetString("Language", "English");

        infoText.text = language == "English" ? englishText : turkishText;

    }
}
