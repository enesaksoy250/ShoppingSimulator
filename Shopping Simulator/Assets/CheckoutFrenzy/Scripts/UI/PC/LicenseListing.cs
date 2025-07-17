using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class LicenseListing : MonoBehaviour
    {
        [SerializeField, Tooltip("Text displaying the license name.")]
        private TMP_Text nameText;

        [SerializeField, Tooltip("Text displaying the license price.")]
        private TMP_Text priceText;

        [SerializeField, Tooltip("Text displaying the license description (products it unlocks).")]
        private TMP_Text descriptionText;

        [SerializeField, Tooltip("Text displaying the license purchase requirements.")]
        private TMP_Text requirementText;

        [SerializeField, Tooltip("Button to purchase the license.")]
        private Button purchaseButton;

        private License license;

        /// <summary>
        /// Initializes the license listing with the license's details.
        /// </summary>
        /// <param name="license">The License object to display.</param>
        public void Initialize(License license)
        {
            this.license = license;

            nameText.text = license.Name;
            string priceTxt = LanguageManager.instance.GetLocalizedValue("PriceText");
            priceText.text = $"{priceTxt} ${license.Price:N2}";
            string descriptionTxt = LanguageManager.instance.GetLocalizedValue("PermissionToSellText");
            descriptionText.text = descriptionTxt;

            // Add each product unlocked by the license to the description text.
            foreach (var product in license.Products)
            {
                descriptionText.text += $"\n\u2022 {product.Name} ({product.Section})";
            }

            // Disable purchasing if the license is already owned.
            if (license.IsOwnedByDefault || license.IsPurchased)
            {
                DisablePurchasing();
            }
            else
            {
                // Subscribe to the OnLevelUp event to update purchase availability.
                DataManager.Instance.OnLevelUp += UpdatePurchaseAvailability;
                UpdatePurchaseAvailability(DataManager.Instance.Data.CurrentLevel); // Initial check.
            }
        }

        /// <summary>
        /// Disables the purchase button and updates the requirement text when a license is already owned.
        /// </summary>
        private void DisablePurchasing()
        {
            purchaseButton.gameObject.SetActive(false);
            string requirementTxt = LanguageManager.instance.GetLocalizedValue("LicenseAlreadyOwnedText");
            requirementText.text = requirementTxt;
        }

        /// <summary>
        /// Updates the purchase availability (interactability of the purchase button) based on the player / store level.
        /// </summary>
        /// <param name="level">The player / store current level.</param>
        private void UpdatePurchaseAvailability(int level)
        {
            string requiresText = LanguageManager.instance.GetLocalizedValue("RequiredLevelText");
       

            if (level >= license.Level)
            {
                string avaliableText = LanguageManager.instance.GetLocalizedValue("AvailableText");
                
                requirementText.text = $"<color=green>{avaliableText}\n{requiresText} {license.Level}";
                purchaseButton.interactable = true;
                string buttonText =LanguageManager.instance.GetLocalizedValue("BuyButtonText");
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
                purchaseButton.onClick.AddListener(() => HandlePurchase(license)); // Add purchase listener.
                DataManager.Instance.OnLevelUp -= UpdatePurchaseAvailability; // Unsubscribe after availability confirmed.
            }
            else
            {
                string buttonText = LanguageManager.instance.GetLocalizedValue("BuyButtonText");
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
                string unavailableText =LanguageManager.instance.GetLocalizedValue("UnavailableText");
                requirementText.text = $"<color=red>{unavailableText}\n{requiresText} {license.Level}";
                purchaseButton.interactable = false;
            }
        }

        /// <summary>
        /// Handles the purchase of the license.
        /// </summary>
        /// <param name="license">The License being purchased.</param>
        private void HandlePurchase(License license)
        {
            bool isPurchased = StoreManager.Instance.PurchaseLicense(license); // Attempt purchase.
            if (isPurchased) DisablePurchasing(); // Update UI if purchase successful.
        }
    }
}
