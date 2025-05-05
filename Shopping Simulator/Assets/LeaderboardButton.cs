using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardButton : MonoBehaviour
{

    Button leaderboardButton;

    private void Awake()
    {
        leaderboardButton = GetComponent<Button>();
    }

    void Start()
    {
        leaderboardButton.onClick.AddListener(delegate { GetBestPlayers(); });
    }

    private void GetBestPlayers()
    {
        PanelManager.instance.LoadPanel("LeaderboardPanel");
       
        RemoveOldUserPanels();
       
        Transform parent = GameObject.FindWithTag("LeaderboardPanel").transform;
        GameObject loadingImage = Instantiate(UIRepository.Instance.LoadingImage,parent);
        RectTransform rectTransform = loadingImage.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
      
        DatabaseManager.Instance.GetBestPlayers(() => Destroy(loadingImage));
    }
    
    private void RemoveOldUserPanels()
    {
        GameObject[] userPanels = GameObject.FindGameObjectsWithTag("UserPanel");
        foreach (var item in userPanels)
        {
            Destroy(item);
        }
    }

}
