using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GamePanelManager : MonoBehaviour
{

    [SerializeField] GameObject[] panels;

    public static GamePanelManager instance;

    private void Awake()
    {
        instance = this;
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

    public void CloseAllPanel()
    {
        foreach(GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }

    public bool IsThereOpenPanel()
    {
        foreach(GameObject panel in panels)
        {
            if (panel.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

}
