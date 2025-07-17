using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryingSnow.CheckoutFrenzy
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField, Tooltip("The ScreenFader component to handle scene transitions.")]
        private ScreenFader screenFader;

        [SerializeField, Tooltip("The background music to play on the main menu.")]
        private AudioClip backgroundMusic;

        [SerializeField]  GameObject loadingPanel;

        private void Start()
        {
            AudioManager.Instance.PlayBGMQueue();
        }

        /// <summary>
        /// Starts the game, fading out the main menu and loading the next scene asynchronously.
        /// </summary>
        public void StartGame()
        {
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
                    asyncLoad.allowSceneActivation = true; // Activate the scene when it's almost fully loaded.
                }

                yield return null; // Wait for the next frame.
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
