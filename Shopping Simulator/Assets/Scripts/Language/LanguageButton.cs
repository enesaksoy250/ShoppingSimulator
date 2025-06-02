using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{

    Button languageButton;
    TextMeshProUGUI languageText;

    private void Awake()
    {
        languageButton = GetComponent<Button>();
        //languageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {

        switch (gameObject.name)
        {
           
            case "RightButton":
                languageButton.onClick.AddListener(delegate { ChangeLanguage("right"); });
                break;
            case "LeftButton":
                languageButton.onClick.AddListener(delegate { ChangeLanguage("left"); });
                break;
        }

    }



    private void ChooseLanguage()
    {
        PlayerPrefs.SetString("Language",LanguageManager.instance.GetSelectedLanguage());
        LanguageManager.instance.LoadLocalizedText(LanguageManager.instance.GetSelectedLanguage());

        UILanguageManager[] uiLanguageManagers = FindObjectsOfType<UILanguageManager>();

        foreach (UILanguageManager manager in uiLanguageManagers)
        {
            manager.UpdateText();
        }

    }

    private void ChangeLanguage(string direction)
    {
        int index = LanguageManager.instance.selectedIndex;
        int max = LanguageManager.GetNumberOfLanguage() - 1;

        if (direction == "right" && index < max)
        {
            LanguageManager.instance.selectedIndex++;
        }
            
        else if (direction == "left" && index > 0)
        {
            LanguageManager.instance.selectedIndex--;
        }

        ChooseLanguage();

        if(UIRepository.Instance.LanguageText.IsActive())
            UIRepository.Instance.LanguageText.text = LanguageManager.instance.GetSelectedLanguage();

        if(UIRepository.Instance.SettingsLanguageText.IsActive())
           UIRepository.Instance.SettingsLanguageText.text = LanguageManager.instance.GetSelectedLanguage();
    }
 
}