using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayAdButton : MonoBehaviour
{
    Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { PlayRewardedAd(); });
    }

    private void PlayRewardedAd() 
    {
        if (CheckInternetConnection.GetInternetConnection())
        {
            GamePanelManager.Instance.LoadPanel("LoadingPanel");
            AdManager.instance.LoadRewardedAd();
        }
        else
        {
            GamePanelManager.Instance.LoadPanel("ErrorPanel"); 
        }
    }
    
}
