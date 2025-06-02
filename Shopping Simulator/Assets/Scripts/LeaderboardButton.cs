using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardButton : MonoBehaviour
{

    public static LeaderboardButton Instance { get; private set; }

    Button leaderboardButton;

    private void Awake()
    {
        Instance = this;
        leaderboardButton = GetComponent<Button>();
    }

    void Start()
    {
        leaderboardButton.onClick.AddListener(delegate { ForLeaderboardButton(); });
    }

    
    public void ForLeaderboardButton()
    {
     
        PanelManager.instance.LoadPanel("LeaderboardPanel");

        if (!PlayerPrefs.HasKey("SetNickname"))
        {
            PanelManager.instance.LoadPanel("LeaderboardInfoPanel");
            return;
        }

        GetBestPlayers();

    }

    public void GetBestPlayers()
    {  
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

        if (userPanels.Length > 0)
        {
            foreach (var item in userPanels)
            {
                Destroy(item);
            }
        }
       
    }

}
