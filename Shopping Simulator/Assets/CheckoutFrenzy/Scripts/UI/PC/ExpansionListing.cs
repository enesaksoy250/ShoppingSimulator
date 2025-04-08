using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    public class ExpansionListing : MonoBehaviour
    {
        [SerializeField, Tooltip("Text displaying the expansion name.")]
        private TMP_Text nameText;

        [SerializeField, Tooltip("Text displaying the expansion price.")]
        private TMP_Text priceText;

        [SerializeField, Tooltip("Text displaying the expansion description.")]
        private TMP_Text descriptionText;

        [SerializeField, Tooltip("Text displaying the expansion requirements.")]
        private TMP_Text requirementText;

        [SerializeField, Tooltip("Button to purchase the expansion.")]
        private Button purchaseButton;

        private Expansion expansion;

        /// <summary>
        /// Initializes the expansion listing with the expansion's details.
        /// </summary>
        /// <param name="expansion">The Expansion object to display.</param>
        public void Initialize(Expansion expansion)
        {
            this.expansion = expansion;

            string priceTxt = LanguageControl.CheckLanguage("Fiyat:","Price:");
            nameText.text = expansion.Name;
            priceText.text = $"{priceTxt} ${expansion.UnlockPrice:N2}";

            string description = LanguageControl.CheckLanguage("Mağazanıza daha fazla yer ekleyin", "Add more room to your store");
            descriptionText.text = $"\u2022 {description}";

            if (expansion.AdditionalCustomers > 0)
            {
                string customerTxt = LanguageControl.CheckLanguage("Müşteri kapasitesi ", "Customer capacity");
                descriptionText.text += $"\n\u2022 {customerTxt} +{expansion.AdditionalCustomers}";
            }

            if (StoreManager.Instance.IsExpansionPurchased(expansion))
            {
                DisablePurchasing();
            }
            else
            {
                DataManager.Instance.OnLevelUp += UpdateRequirements;
                StoreManager.Instance.OnExpansionPurchased += UpdateRequirements;

                UpdateRequirements(0);
            }
        }

        /// <summary>
        /// Disables the purchase button and updates the requirement text when an expansion is already owned.
        /// </summary>
        private void DisablePurchasing()
        {
            purchaseButton.gameObject.SetActive(false);
            requirementText.text = LanguageControl.CheckLanguage("Bu genişleme zaten sizin.", "You already own this expansion.");
        }

        /// <summary>
        /// Updates the requirement text and interactability of the purchase button based on current game state.
        /// </summary>
        /// <param name="_">Parameter is not used but required by the event signature.</param>
        private void UpdateRequirements(int _)
        {
            bool isLevelMet = DataManager.Instance.Data.CurrentLevel >= expansion.RequiredLevel;
            bool isCurrentExpansion = StoreManager.Instance.IsCurrentExpansion(expansion);

            bool isAvailable = isLevelMet && isCurrentExpansion;
            string requirement = LanguageControl.CheckLanguage("KULLANILABİLİR", "AVAILABLE");
            string requirement2 = LanguageControl.CheckLanguage("KULLANILAMAZ", "UNAVAILABLE");
            requirementText.text = isAvailable ? $"<color=green>{requirement}" : $"<color=red>{requirement2}";

            requirementText.text += isLevelMet ? "\n<color=green>" : "\n<color=red>";
            string text = LanguageControl.CheckLanguage("Gerekli Seviye", "Requires Level");
            requirementText.text += $"{text} {expansion.RequiredLevel}";

            string text2 = LanguageControl.CheckLanguage("Önceki genişleme satın alındı", "Previous expansion purchased");
            string text3 = LanguageControl.CheckLanguage("Önceki genişletmeyi satın alın ", "Purchase previous expansion");

            if (isCurrentExpansion) requirementText.text += $"\n<color=green>{text2}";
            else requirementText.text += $"\n<color=red>{text3}";

            string buttonText = LanguageControl.CheckLanguage("Satın Al", "Purchase");
            purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

            purchaseButton.interactable = isAvailable;

            if (isAvailable)
            {
            
                purchaseButton.onClick.AddListener(HandlePurchase);
                DataManager.Instance.OnLevelUp -= UpdateRequirements;
                StoreManager.Instance.OnExpansionPurchased -= UpdateRequirements;
            }
        }

        /// <summary>
        /// Handles the purchase of the expansion.
        /// </summary>
        private void HandlePurchase()
        {
            bool isPurchased = StoreManager.Instance.PurchaseExpansion();
            if (isPurchased) DisablePurchasing();
        }
    }
}
