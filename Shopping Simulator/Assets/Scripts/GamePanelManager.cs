using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePanelManager : MonoBehaviour
{

    [SerializeField] GameObject[] panels;

    public static GamePanelManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        if (!PlayerPrefs.HasKey("RemoveAd") && PlayerPrefs.HasKey("TutorialEnd"))
        {

            int showAdPanel = PlayerPrefs.GetInt("ShowAdPanel", 0);
            int showRemoveAdPanel = PlayerPrefs.GetInt("ShowRemoveAdPanel", 1);

            showAdPanel++;
            showRemoveAdPanel++;

            if (showAdPanel % 2 == 0)
            {
                StartCoroutine(ShowAdPanel());
            }

            if (showRemoveAdPanel % 2 == 0)
            {
                int time = showRemoveAdPanel > 2 ? 2 : 90;
                StartCoroutine(ShowRemoveAdPanel(time));
            }

            PlayerPrefs.SetInt("ShowAdPanel", showAdPanel);
            PlayerPrefs.SetInt("ShowRemoveAdPanel", showRemoveAdPanel);

        }
    }

    IEnumerator ShowAdPanel()
    {
        yield return new WaitForSeconds(2);
        LoadPanel("AdPanel");
    }

    IEnumerator ShowRemoveAdPanel(int time)
    {
        yield return new WaitForSeconds(time);
        LoadPanel("RemoveAdPanel");
    }


    public void LoadPanel(string panelName)
    {
        foreach (GameObject panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);

                if(panel.name == "StorePanel")
                {
                    if (PlayerPrefs.GetInt("RemoveAd") == 1)
                    {
                        UIManager.Instance.RemoveAdPanel.GetComponent<Button>().interactable = false;
                        string language = LanguageManager.GetLanguage();
                        UIManager.Instance.StorePriceText.text = language == "English" ? "PURCHASED" : "SATIN ALINDI";
                    }
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

}
