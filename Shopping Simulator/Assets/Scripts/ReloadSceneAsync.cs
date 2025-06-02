using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneAsync : MonoBehaviour
{
   
    public static ReloadSceneAsync instance;

    [SerializeField] GameObject loadingPanel;

    [SerializeField] ScreenFader screenFader;
 
    private void Awake()
    {
        instance = this;
    }

    public void RestartGame()
    {

        DataManager.Instance.SaveGameData();

        screenFader.FadeIn(onComplete: () => // Fade in the screen.
            StartCoroutine(StartGameAsync()) // Start the asynchronous scene loading coroutine when the fade in is complete.
        );

        AudioManager.Instance.PlaySFX(AudioID.Click);
    }

    private IEnumerator StartGameAsync()
    {

        loadingPanel.SetActive(true);
        //loadingPanel.GetComponentInChildren<TextMeshProUGUI>().text = PlayerPrefs.GetString("Language") == "English" ? "Loading..." : "Yükleniyor...";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1); // Load Game scene (index 1) asynchronously.
        asyncLoad.allowSceneActivation = false; // Prevent automatic scene activation.

        // Wait until the asynchronous scene fully loads.
        while (!asyncLoad.isDone)
        {
            // Scene has loaded as much as possible,
            // the last 10% can't be multi-threaded.
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
                loadingPanel.SetActive(false);              
                // Activate the scene when it's almost fully loaded.
            }

            yield return null; // Wait for the next frame.
        }

       
    

    }
}
