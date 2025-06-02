using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
   
    public static PanelManager instance;
    
    [SerializeField] GameObject[] panels;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Login"))
        {
            ChangePanelVisibility("LanguagePanel", true);
            LanguageManager.SetLanguagePanel();
        }
    }

    public void ChangePanelVisibility(string panelName,bool state)
    {
        foreach (GameObject panel in panels)
        {
            if(panel.name == panelName)
            {
                panel.SetActive(state);
            }
        }
    }

   public void LoadPanel(string panelName)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);
            }
        }
    }

    public void LoadPanel(string panelName,string message = null)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);

                if(message != null)
                {
                    panel.GetComponentInChildren<TextMeshProUGUI>().text = message;
                }
            }
        }
    }

    public void ClosePanel(string panelName)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(false);
            }
        }
    }

    public void SelectLanguageButton(string buttonName)
    {

        PlayerPrefs.SetString("Language", buttonName);

        LanguageManager.instance.LoadLocalizedText(PlayerPrefs.GetString("Language"));

        UILanguageManager[] uiLanguageManagers = FindObjectsOfType<UILanguageManager>();

        foreach (UILanguageManager manager in uiLanguageManagers)
        {

            manager.UpdateText();

        }

    }

}
