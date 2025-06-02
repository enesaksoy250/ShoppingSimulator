﻿using System.Linq;
using UnityEngine;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    /// <summary>
    /// A shelf for storing and displaying products in the store.
    /// Inherits from the ProductContainer class.
    /// </summary>
    public class Shelf : ProductContainer
    {
        [SerializeField, Tooltip("Text displaying product information")]
        private TMP_Text infoText;

        /// <summary>
        /// The shelving unit this shelf belongs to.
        /// </summary>
        public ShelvingUnit ShelvingUnit { get; set; }

        /// <summary>
        /// Enables or disables interaction with the shelf collider.
        /// </summary>
        /// <param name="enabled">True to enable interaction, false to disable.</param>
        public void ToggleInteraction(bool enabled) => boxCollider.enabled = enabled;

        private void Awake()
        {
            // Set the layer of the shelf GameObject
            gameObject.layer = GameConfig.Instance.ShelfLayer.ToSingleLayer();
        }

        private void Start()
        {
            // Subscribe to the OnPriceChanged event from StoreManager
            DataManager.Instance.OnPriceChanged += HandlePriceChanged;
            UpdateInfoText();
        }

        private void OnDisable()
        {
            // Unsubscribe from the OnPriceChanged event when disabled
            DataManager.Instance.OnPriceChanged -= HandlePriceChanged;
        }

        /// <summary>
        /// Handles price changes for the product on this shelf.
        /// </summary>
        /// <param name="productId">The ID of the product whose price changed.</param>
        private void HandlePriceChanged(int productId)
        {
            if (Product != null && Product.ProductID == productId)
            {
                UpdateInfoText();
            }
        }

        /// <summary>
        /// Attempts to place a product model on the shelf.
        /// </summary>
        /// <param name="productModel">The product model GameObject to place.</param>
        /// <param name="productPosition">Will be set to the position where the product was placed, or Vector3.zero if placement failed.</param>
        /// <returns>True if the product was placed successfully, false if the shelf is full.</returns>
        public bool PlaceProductModel(GameObject productModel, out Vector3 productPosition)
        {
            productPosition = Vector3.zero;

            if (Quantity < Capacity)
            {
                productModels.Add(productModel);
                productPosition = productPositions[Quantity - 1];
                UpdateInfoText();
                return true;
            }
            else
            {
                string text = LanguageManager.instance.GetLocalizedValue("ShelfIsFullText");
                UIManager.Instance.Message.Log(text);
                return false;
            }
        }

        /// <summary>
        /// Takes the last placed product model from the shelf.
        /// </summary>
        /// <returns>The last placed product model GameObject, or null if the shelf is empty.</returns>
        public GameObject TakeProductModel()
        {
            var productModel = productModels.LastOrDefault();

            if (productModel != null)
            {
                productModels.Remove(productModel);

                if (Quantity == 0) Product = null;

                UpdateInfoText();
            }

            return productModel;
        }

        /// <summary>
        /// Updates the text displaying product information on the shelf.
        /// </summary>
        private void UpdateInfoText()
        {
            if (infoText == null) return;

            if (Product != null)
            {
                decimal price = DataManager.Instance.GetCustomProductPrice(Product);
                infoText.text = $"[{Quantity}/{Capacity}] ${price:F2}";
            }
            else
            {
                infoText.text = $"[-/-] $--.--";
            }
        }
    }
}
