using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{

    ScreenFader screenFader;
    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void Start()
    {
        screenFader = FindObjectOfType<ScreenFader>();
        button.onClick.AddListener(BackToMainMenu);
    }

    public void BackToMainMenu()
    {
        DataManager.Instance.SaveGameData();

        screenFader.FadeIn(onComplete: () => // Fade in the screen.
            StartCoroutine(StartGameAsync()) // Start the asynchronous scene loading coroutine when the fade in is complete.
        );

        AudioManager.Instance.PlaySFX(AudioID.Click);
    }

    private IEnumerator StartGameAsync()
    {

        GamePanelManager.instance.LoadPanel("SceneLoadingPanel");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0); // Load Game scene (index 1) asynchronously.
        asyncLoad.allowSceneActivation = false; // Prevent automatic scene activation.

        // Wait until the asynchronous scene fully loads.
        while (!asyncLoad.isDone)
        {
            // Scene has loaded as much as possible,
            // the last 10% can't be multi-threaded.
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true; // Activate the scene when it's almost fully loaded.
            }

            yield return null; // Wait for the next frame.
        }
    }
}
