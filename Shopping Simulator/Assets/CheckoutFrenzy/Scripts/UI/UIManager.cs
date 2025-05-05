using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleInputNamespace;

namespace CryingSnow.CheckoutFrenzy
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Controls")]
        [SerializeField, Tooltip("Image used for the player's crosshair.")]
        private Image crosshair;

        [SerializeField, Tooltip("Joystick component for player movement input.")]
        private Joystick joystick;

        [SerializeField, Tooltip("Button for interacting with interactables.")]
        private Button interactButton;

        [Header("UI Components")]
        [SerializeField, Tooltip("Component to display information about a box.")]
        private BoxInfo boxInfo;

        [SerializeField, Tooltip("Component to display in-game messages.")]
        private Message message;

        [SerializeField, Tooltip("Component to customize product prices.")]
        private PriceCustomizer priceCustomizer;

        [SerializeField, Tooltip("Component to display the PC monitor UI.")]
        private PCMonitor pcMonitor;

        [SerializeField, Tooltip("Component to manage the cash register UI.")]
        private CashRegister cashRegister;

        [SerializeField, Tooltip("Component to manage the payment terminal UI.")]
        private PaymentTerminal paymentTerminal;

        [SerializeField, Tooltip("Component to display the summary screen UI.")]
        private SummaryScreen summaryScreen;

        [SerializeField, Tooltip("Component to manage the skip dialog UI.")]
        private SkipDialog skipDialog;

        [SerializeField, Tooltip("Component to manage the virtual keyboard UI.")]
        private VirtualKeyboard virtualKeyboard;

        [SerializeField, Tooltip("Prefab for the chat bubble UI element.")]
        private ChatBubble chatBubblePrefab;

        [Header("Gameplay UI")]
        [SerializeField, Tooltip("Text component to display the delivery timer.")]
        private TMP_Text deliveryTimerText;

        [SerializeField, Tooltip("Image used as a radial fill bar to visualize hold progress (e.g., to interact and move furniture).")]
        private Image holdProgressImage;

        [Header("Action UIs")]
        [SerializeField, Tooltip("Parent for action buttons (Mobile).")]
        private RectTransform actionButtonsParent;

        [SerializeField, Tooltip("Parent for action prompts (PC).")]
        private RectTransform actionPromptsParent;

        [Header("Settings UI")]
        [SerializeField, Tooltip("Component to manage game settings.")]
        private SettingsWindow settingsWindow;

        [SerializeField, Tooltip("Key to display settings window.")]
        private KeyCode settingsKey = KeyCode.Escape;

        [Header("Others")]
        [SerializeField] private TextMeshProUGUI mixAdLoadingText;
        [SerializeField] private TextMeshProUGUI storePriceText;
        [SerializeField] private GameObject removeAdPanel;
        public Message Message => message;
        public PriceCustomizer PriceCustomizer => priceCustomizer;
        public PCMonitor PCMonitor => pcMonitor;
        public CashRegister CashRegister => cashRegister;
        public PaymentTerminal PaymentTerminal => paymentTerminal;
        public SummaryScreen SummaryScreen => summaryScreen;
        public SkipDialog SkipDialog => skipDialog;
        public InteractMessage InteractMessage { get; private set; }
        public VirtualKeyboard VirtualKeyboard => virtualKeyboard;
        public TextMeshProUGUI MixAdLoadingText => mixAdLoadingText;
        public TextMeshProUGUI StorePriceText => storePriceText;
        public GameObject RemoveAdPanel => removeAdPanel;
  
        private Canvas canvas;
        private bool isMobileControl;
        private List<IActionUI> actionUIs;

        private void Awake()
        {
            Instance = this;

            canvas = GetComponentInChildren<Canvas>();
        }

        private void Start()
        {
            isMobileControl = GameConfig.Instance.ControlMode == ControlMode.Mobile;
        

            if (isMobileControl)
            {
                actionUIs = actionButtonsParent.GetComponentsInChildren<IActionUI>().ToList();

                actionPromptsParent.gameObject.SetActive(false);
            }
            else
            {
                actionUIs = actionPromptsParent.GetComponentsInChildren<IActionUI>().ToList();

                actionButtonsParent.gameObject.SetActive(false);

                Vector2 originalPos = actionPromptsParent.anchoredPosition;
                Vector2 targetPos = new Vector2(0f, originalPos.y);
                actionPromptsParent.anchoredPosition = targetPos;
            }

            this.InteractMessage = GetComponentsInChildren<InteractMessage>(true)
                .FirstOrDefault(m => m.ControlMode == GameConfig.Instance.ControlMode);

            HideBoxInfo();

            ToggleInteractButton(false);
            ToggleCrosshair(true);

            actionUIs.ForEach(actionUI => actionUI.SetActive(false));

            ToggleDeliveryTimer(false);
            UpdateHoldProgress(0f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(settingsKey))
            {
                if (settingsWindow.gameObject.activeSelf) settingsWindow.Close();
                else settingsWindow.Open();
            }
        }

        public void ToggleInteractButton(bool active)
        {
            if (active && !isMobileControl) return;

            interactButton.gameObject.SetActive(active);
        }

        public void ToggleCrosshair(bool active)
        {
            crosshair.gameObject.SetActive(active);
            joystick.gameObject.SetActive(active);

            if (!isMobileControl)
            {
                Cursor.visible = !active;
                Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }

        public void ToggleActionUI(ActionType actionType, bool active, System.Action action)
        {
            var actionUI = actionUIs.FirstOrDefault(a => a.ActionType == actionType);

            actionUI.SetActive(active);

            actionUI.OnClick.RemoveAllListeners();
            actionUI.OnClick.AddListener(() => action?.Invoke());
        }

        public void DisplayBoxInfo(Box box)
        {
            boxInfo.gameObject.SetActive(true);
            boxInfo.UpdateInfo(box);
        }

        public void HideBoxInfo()
        {
            boxInfo.gameObject.SetActive(false);
        }

        public void UpdateHoldProgress(float progress)
        {
            if (progress > 0.2f)
            {
                if (!holdProgressImage.gameObject.activeSelf)
                {
                    holdProgressImage.gameObject.SetActive(true);
                }
                holdProgressImage.fillAmount = Mathf.Clamp01(progress);
            }
            else
            {
                holdProgressImage.gameObject.SetActive(false);
            }
        }

        public void UpdateDeliveryTimer(int time)
        {
            ToggleDeliveryTimer(true);

            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
            string deliveryText = LanguageControl.CheckLanguage("Sonraki Teslimat:", "Next Delivery:");
            deliveryTimerText.text = $"{deliveryText}\n{timeSpan:mm\\:ss}";

            if (time <= 0) ToggleDeliveryTimer(false);
        }

        private void ToggleDeliveryTimer(bool active)
        {
            GameObject deliveryTimer = deliveryTimerText.transform.parent.gameObject;
            deliveryTimer.SetActive(active);
        }

        public ChatBubble ShowChatBubble(string chat, Transform speaker)
        {
            var chatBubble = Instantiate(chatBubblePrefab, canvas.transform, false);
            chatBubble.transform.SetAsFirstSibling();
            chatBubble.Show(chat, speaker);
            return chatBubble;
        }
    }
}
