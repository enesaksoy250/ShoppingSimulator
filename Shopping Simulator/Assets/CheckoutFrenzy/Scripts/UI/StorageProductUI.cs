using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class StorageProductUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI quantityText;

        private Button button;

        public void SetInteractable(bool value) => button.interactable = value;

        public void Initialize(Shelf shelf, Product product, int quantity, System.Action<StorageProductUI> onClick)
        {
            iconImage.sprite = product.Icon;
            nameText.text = product.Name;
            quantityText.text = $"x{quantity}";

            button = GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                shelf.SetLabel(product);
                button.interactable = false;
                onClick?.Invoke(this);
            });
        }
    }
}
