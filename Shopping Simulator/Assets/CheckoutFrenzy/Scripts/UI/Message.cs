using UnityEngine;
using TMPro;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Message : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private TMP_Text messageText;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            messageText = GetComponentInChildren<TMP_Text>();

            canvasGroup.alpha = 0f;
            messageText.text = "";
        }

        /// <summary>
        /// Displays a message with an optional color and display time.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="color">The color of the message text (optional, defaults to white).</param>
        /// <param name="time">The duration (in seconds) to display the message (defaults to 2 seconds).</param>
        public void Log(string message, Color? color = null, float time = 2f)
        {
            Color displayColor = color ?? Color.white; // Use the provided color or white if none given.

            // Format the message with the specified color.
            messageText.text = $"<color=#{ColorUtility.ToHtmlStringRGBA(displayColor)}>"; // Set the text color.
            messageText.text += message; // Append the message.

            DOTween.Kill(canvasGroup); // Kill any existing tweens on the canvas group to prevent conflicts.

            // Fade in the message.
            canvasGroup.DOFade(1f, 0.5f)
                .OnComplete(() => // After fading in:
                {
                    // Fade out the message after a delay.
                    canvasGroup.DOFade(0f, 0.5f)
                        .SetDelay(time); // Set the delay before fading out.
                });
        }
    }
}
