using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckInternetConnection : MonoBehaviour
{

    private bool isOpen = false;
    private bool isConnected = true;
    private int sceneIndex;

    public static CheckInternetConnection instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
       
        isConnected = Application.internetReachability != NetworkReachability.NotReachable;
        sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (!isConnected && !isOpen)
        {
            if(sceneIndex == 0)
            {
                PanelManager.instance.ChangePanelVisibility("NotConnectPanel", true);
            }

            else if(sceneIndex == 1)
            {
                GamePanelManager.Instance.LoadPanel("NotConnectPanel");
            }
            
            isOpen = true;
        }

        else if(isConnected && isOpen)
        {
            if (sceneIndex == 0)
            {
                PanelManager.instance.ChangePanelVisibility("NotConnectPanel", false);
            }

            else if (sceneIndex == 1)
            {
                GamePanelManager.Instance.ClosePanel("NotConnectPanel");
            }
      
            isOpen = false;
        }
    }

    public static bool GetInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

}
