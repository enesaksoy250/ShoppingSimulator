using System.Collections.Generic;
using UnityEngine;

namespace CryingSnow.CheckoutFrenzy
{
    [CreateAssetMenu()]
    public class License : ScriptableObject
    {
        [SerializeField] private new string name;
        [SerializeField] private int price;
        [SerializeField] private int level;
        [SerializeField] private bool isOwnedByDefault;
        [SerializeField] private List<Product> products;

        public bool IsOwnedByDefault => isOwnedByDefault;
        public bool IsPurchased => CheckPurchased();

        public string Name => name;
        public int Price => price;
        public int Level => level;
        public List<Product> Products => products;

        private bool CheckPurchased()
        {
            foreach (var product in products)
            {
                if (!DataManager.Instance.Data.LicensedProducts.Contains(product.ProductID))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
