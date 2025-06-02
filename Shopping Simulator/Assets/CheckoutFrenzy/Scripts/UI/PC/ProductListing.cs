using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.UIElements;

namespace CryingSnow.CheckoutFrenzy
{
    public class ProductListing : MonoBehaviour
    {
        [SerializeField, Tooltip("Image displaying the product icon.")]
        private Image iconImage;

        [SerializeField, Tooltip("Text displaying the product name.")]
        private TMP_Text nameText;

        [SerializeField, Tooltip("Text displaying the product category.")]
        private TMP_Text categoryText;

        [SerializeField, Tooltip("Text displaying the quantity per box/pack.")]
        private TMP_Text quantityText;

        [SerializeField, Tooltip("Text displaying the product section.")]
        private TMP_Text sectionText;

        [SerializeField, Tooltip("Text displaying the price per box/pack.")]
        private TMP_Text priceText;

        [SerializeField, Tooltip("Text displaying the selected amount of boxes/packs.")]
        private TMP_Text amountText;

        [SerializeField, Tooltip("Text displaying the total price of the selected amount.")]
        private TMP_Text totalText;

        [SerializeField, Tooltip("Button to decrease the selected amount.")]
        private Button decreaseButton;

        [SerializeField, Tooltip("Button to increase the selected amount.")]
        private Button increaseButton;

        [SerializeField, Tooltip("Button to add the selected product amount to the cart.")]
        private Button addToCartButton;

        private int amount;
        private int boxQuantity;
        private decimal singlePrice;

        /// <summary>
        /// Initializes the product listing with the product's details.
        /// </summary>
        /// <param name="product">The Product object to display.</param>
        public void Initialize(Product product)
        {
            iconImage.sprite = product.Icon;
            nameText.text = product.Name;

            // Format the category name for display (e.g., "ProductCategory" to "Product Category").
            var categoryName = product.ProductCategory.ToString();
            var formattedName = Regex.Replace(categoryName, @"([a-z])([A-Z])", "$1 $2"); // Add spaces between words.
            formattedName = Regex.Replace(formattedName, @"\bAnd\b", "&"); // Replace "And" with "&".
            categoryText.text = formattedName;

            boxQuantity = product.GetBoxQuantity();
            string pack = LanguageManager.instance.GetLocalizedValue("PackText");
            quantityText.text = $"<sprite=12> <size=30>{boxQuantity} {pack}"; // Set the quantity text.

            string section = LanguageManager.instance.GetLocalizedValue("SectionText");
            sectionText.text = $"{section} {product.Section}";

            string price = LanguageManager.instance.GetLocalizedValue("PriceText");
            singlePrice = product.Price * boxQuantity; // Calculate the price per box/pack.
            priceText.text = $"{price} ${singlePrice:N2}";

            UpdateAmount(1, false); // Initialize amount to 1.

            decreaseButton.onClick.AddListener(() => UpdateAmount(-1)); // Add listener to decrease button.
            increaseButton.onClick.AddListener(() => UpdateAmount(1));  // Add listener to increase button.

            string cartText = LanguageManager.instance.GetLocalizedValue("AddToCartText");
            addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = cartText;
            addToCartButton.onClick.AddListener(() => PC.Instance.AddToCart(product, amount)); // Add listener to add to cart button.
        }

        /// <summary>
        /// Updates the selected amount and the total price.
        /// </summary>
        /// <param name="value">The amount to change the selected quantity by (positive or negative).</param>
        /// <param name="playSFX">Whether to play a sound effect (defaults to true).</param>
        private void UpdateAmount(int value, bool playSFX = true)
        {
            amount += value;
            amount = Mathf.Clamp(amount, 1, 10);
            string amountTxt = LanguageManager.instance.GetLocalizedValue("AmountText");// Clamp the amount between 1 and 10.
            amountText.text = $"{amountTxt} {amount}";

            string total = LanguageManager.instance.GetLocalizedValue("TotalText");
            decimal totalPrice = singlePrice * amount; // Calculate the total price.
            totalText.text = $"{total} ${totalPrice:N2}";

            if (playSFX) AudioManager.Instance.PlaySFX(AudioID.Click); // Play a click sound effect.
        }
    }
}
