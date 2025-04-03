using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    public class CashRegister : MonoBehaviour
    {
        [SerializeField, Tooltip("The button used to clear the cash register input.")]
        private Button clearButton;

        [SerializeField, Tooltip("The button used to confirm the transaction.")]
        private Button confirmButton;

        public event System.Action<int> OnDraw;
        public event System.Action OnClear;
        public event System.Action OnConfirm;

        private RectTransform rect;
        private float originalPosY;
        private bool allowDrawing;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            originalPosY = rect.anchoredPosition.y;

            // Add listeners to the clear and confirm buttons to invoke the corresponding events.
            clearButton.onClick.AddListener(() => OnClear?.Invoke());
            confirmButton.onClick.AddListener(() => OnConfirm?.Invoke());
        }

        /// <summary>
        /// Handles drawing money from the cash register.
        /// </summary>
        /// <param name="amount">The amount of money to draw (in cents).</param>
        public void Draw(int amount)
        {
            // Prevent drawing if it's not allowed (e.g., while the register is closing).
            if (!allowDrawing) return;

            OnDraw?.Invoke(amount);

            // Play different sound effects based on the amount drawn.
            AudioID audioId = amount < 100 ? AudioID.Coin : AudioID.Draw;
            AudioManager.Instance.PlaySFX(audioId);
        }

        /// <summary>
        /// Opens the cash register UI, allowing money to be drawn.
        /// </summary>
        public void Open()
        {
            // Use DOTween to smoothly animate the cash register opening.
            rect.DOAnchorPosY(0f, 0.5f)
                .OnComplete(() => allowDrawing = true); // Enable drawing after the animation completes.
        }

        /// <summary>
        /// Closes the cash register UI, preventing further drawing.
        /// </summary>
        public void Close()
        {
            allowDrawing = false; // Disable drawing.
            rect.DOAnchorPosY(originalPosY, 0.5f); // Animate the cash register closing.
        }
    }
}
