using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using TMPro;
using DG.Tweening;

namespace CryingSnow.CheckoutFrenzy
{
    public class CheckoutCounter : MonoBehaviour, IInteractable
    {
        [SerializeField, Tooltip("The position where the first customer in line stands to check out.")]
        private Vector3 checkoutPoint;

        [SerializeField, Tooltip("The direction of the customer queue.")]
        private Vector3 liningDirection = Vector3.left;

        [SerializeField, Tooltip("The position where items are moved after being scanned.")]
        private Vector3 packingPoint;

        [SerializeField, Tooltip("The position where change is given to the customer.")]
        private Vector3 moneyPoint;

        [SerializeField, Tooltip("The area within these bounds defines where products can be placed.")]
        private Bounds placementBounds;

        [SerializeField, Tooltip("The maximum number of attempts to find a valid placement position for the products.")]
        private int maxPlacementAttempts = 100;

        [SerializeField, Tooltip("The time required for the cashier (if one is hired) to scan a product (in seconds).")]
        private float autoScanTime = 1f;

        [SerializeField, Tooltip("The TextMeshPro object used to display information on the checkout monitor.")]
        private TextMeshPro monitorText;

        [SerializeField, Tooltip("A list of sprites representing different money visuals.")]
        private List<Sprite> moneySprites;

        [SerializeField, Tooltip("The position where the cashier stands while working at this counter.")]
        private Transform cashierPoint;

        [SerializeField, Tooltip("The Cinemachine Virtual Camera used to focus on the counter during transactions.")]
        private CinemachineVirtualCamera cashierCamera;

        public enum State { Standby, Placing, Scanning, CashPay, CardPay }
        public State CurrentState { get; private set; }

        public bool HasCashier { get; private set; }

        public List<Customer> LiningCustomers { get; private set; } = new List<Customer>();
        public int GetQueueNumber(Customer customer) => LiningCustomers.IndexOf(customer);

        // UI References
        private CashRegister cashRegister => UIManager.Instance.CashRegister;
        private PaymentTerminal paymentTerminal => UIManager.Instance.PaymentTerminal;
        private Message message => UIManager.Instance.Message;

        private PlayerController player;
        private Customer currentCustomer;

        private List<CheckoutItem> checkoutItems = new List<CheckoutItem>();

        // Total cost of the customer's items.
        private decimal totalPrice;
        // Amount of money the customer paid.
        private decimal customerMoney;
        // Change given to the customer.
        private Stack<ChangeMoney> givenChange = new Stack<ChangeMoney>();

        // List of available denominations in cents (e.g., $50, $20, $10, $5, $1, and coins)
        private readonly List<int> denominations = new List<int> { 5000, 2000, 1000, 500, 100, 50, 25, 10, 5, 1 };

        private void Awake()
        {
            gameObject.layer = GameConfig.Instance.InteractableLayer.ToSingleLayer();
            UpdateMonitorText();
            //HasCashier = false;
        }

        private void Start()
        {
            StoreManager.Instance.RegisterCounter(this);
        }

        public void Interact(PlayerController player)
        {
            if (HasCashier)
            {
                //string text = LanguageControl.CheckLanguage("Burada zaten bir kasiyer �al���yor.", "A cashier is already working here."); 
                string cashierText = LanguageManager.instance.GetLocalizedValue("CashierText");
                message.Log(cashierText);
                return;
            }

            this.player = player;

            cashierCamera.gameObject.SetActive(true);

            ActivateReturnButton();

            player.StateManager.PushState(PlayerState.Working);

            //UIManager.Instance.ToggleCrosshair(false);

            UIManager.Instance.InteractMessage.Hide();
        }

        public void OnFocused()
        {
            //string text = LanguageControl.CheckLanguage("Kasiyer olarak �deme yapmak i�in dokunun!", "Tap to perform checkout as a cashier");
            string TapToPayAsCashierText = LanguageManager.instance.GetLocalizedValue("TapToPayAsCashierText");
            UIManager.Instance.InteractMessage.Display(TapToPayAsCashierText);
        }

        public void OnDefocused()
        {
            UIManager.Instance.InteractMessage.Hide();
        }


        public void AssignCashier(Cashier cashier)
        {
            HasCashier = cashier != null;

            if (HasCashier)
            {
                cashier.transform.SetParent(cashierPoint);
                cashier.transform.localPosition = Vector3.zero;
                cashier.transform.localRotation = Quaternion.identity;

                if (CurrentState == State.Scanning)
                    StartCoroutine(AutoScan());
            }
            else
            {
                Destroy(cashierPoint.GetChild(0).gameObject);
            }
        }

        /// <summary>
        /// Calculates the position for a customer in the queue.
        /// </summary>
        /// <param name="queueNumber">The customer's position in the queue (0 for the front of the line).</param>
        /// <param name="lookDirection">An output parameter providing the direction the customer should be facing.</param>
        /// <returns>The world position for the specified customer in the queue.</returns>



        public Vector3 GetQueuePosition(Customer customer, out Vector3 lookDirection)
        {
            Vector3 worldCheckoutPoint = transform.TransformPoint(checkoutPoint);

            int queueNumber = GetQueueNumber(customer);

            if (queueNumber > 0) lookDirection = -liningDirection;
            else lookDirection = (cashierPoint.position - worldCheckoutPoint).normalized;

            return worldCheckoutPoint + liningDirection * queueNumber * 0.5f;
        }

        /// <summary>
        /// Places the customer's products onto the checkout counter.
        /// </summary>
        /// <param name="customer">The customer whose products are being placed.</param>
        /// <returns>An IEnumerator for coroutine execution, allowing for placement attempts and animation delays.</returns>
        public IEnumerator PlaceProducts(Customer customer)
        {
            SetCurrentState(State.Placing);

            currentCustomer = customer;

            var products = customer.Inventory;

            foreach (var product in products)
            {
                int attempts = 0;
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                bool placementSuccessful = false;

                while (attempts < maxPlacementAttempts)
                {
                    // Generate a random position within table's placement bounds
                    position.x = Random.Range(placementBounds.min.x, placementBounds.max.x);
                    position.y = placementBounds.min.y;
                    position.z = Random.Range(placementBounds.min.z, placementBounds.max.z);
                    position = transform.TransformPoint(position);

                    // Generate a random rotation on Y-axis
                    rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    // Check for overlaps at the new position and rotation
                    Collider[] colliders = Physics.OverlapBox(position, product.Size / 2, rotation, GameConfig.Instance.CheckoutItemLayer);

                    if (colliders.Length == 0)
                    {
                        placementSuccessful = true;
                        break;
                    }

                    attempts++;
                    yield return null;
                }

                if (!placementSuccessful)
                {
                    Debug.LogWarning($"Could not place product '{product.Model.name}' after {maxPlacementAttempts} attempts. Placing at last attempted position.");
                }

                // Calculate the position in front of the customer where the product will initially appear.
                var customerFront = customer.transform.TransformPoint(new Vector3(0f, 1f, 0.5f));

                // Instantiate the product model at the calculated position, without any initial rotation.
                var productModel = Instantiate(product.Model, customerFront, Quaternion.identity);

                // Initially set the scale of the product model to zero, so it can be animated to its final size.
                productModel.transform.localScale = Vector3.zero;

                // Set the duration of the animation.
                float duration = 0.3f;

                // Animate the product model's jump to the target position, rotation, and scale.
                productModel.transform.DOJump(position, 0.5f, 1, duration);
                productModel.transform.DORotateQuaternion(rotation, duration);
                productModel.transform.DOScale(Vector3.one, duration);

                yield return new WaitForSeconds(duration);

                // Add the CheckoutItem component to the instantiated product model.
                var checkoutItem = productModel.AddComponent<CheckoutItem>();

                // Add the checkout item to the list of checkout items.
                checkoutItems.Add(checkoutItem);

                // Initialize the CheckoutItem component with the product data and the scanning handler.
                checkoutItem.Initialize(product, () => HandleScanning(checkoutItem));
            }

            SetCurrentState(State.Scanning);

            if (HasCashier) StartCoroutine(AutoScan());
        }

        private void HandleScanning(CheckoutItem item)
        {
            UIManager.Instance.ToggleActionUI(ActionType.Return, false, null);

            ScanItem(item);

            DataManager.Instance.AddExperience();

            if (checkoutItems.Count == 0)
            {
                StartCoroutine(ProcessPayment());
            }
        }

        private void ScanItem(CheckoutItem item)
        {
            checkoutItems.Remove(item);

            // Move the scanned item to the packing point and then destroy it.
            item.transform.DOMove(transform.TransformPoint(packingPoint), 0.3f)
                .OnComplete(() => Destroy(item.gameObject));

            decimal price = DataManager.Instance.GetCustomProductPrice(item.Product);
            totalPrice += price;
            UpdateMonitorText();

            if (!HasCashier) AudioManager.Instance.PlaySFX(AudioID.Scanner);

            MissionManager.Instance.UpdateMission(Mission.Goal.Revenue, (int)(price * 100));
            MissionManager.Instance.UpdateMission(Mission.Goal.Sell, 1, item.Product.ProductID);
        }
        private IEnumerator AutoScan()
        {
            yield return new WaitForSeconds(autoScanTime);

            while (checkoutItems.Count > 0)
            {
                var item = checkoutItems.FirstOrDefault();
                if (item != null) { 
                    ScanItem(item);
                    DataManager.Instance.AddExperience();
                } 

                yield return new WaitForSeconds(autoScanTime);
            }

            StartCoroutine(ProcessPayment());
        }

        private IEnumerator ProcessPayment()
        {
            // Determine payment method
            CurrentState = Random.value < 0.6f ? State.CashPay : State.CardPay;
            bool isUsingCash = CurrentState == State.CashPay;

            // Wait for the customer to hand over payment
            yield return currentCustomer.HandsPayment(isUsingCash, HasCashier ? cashierPoint.GetComponentInChildren<Cashier>() : null);

            if (!HasCashier)
            {
                // Setup manual payment system (player as cashier)
                if (isUsingCash)
                {
                    customerMoney = GetRandomPaymentOption();

                    cashRegister.Open();
                    cashRegister.OnDraw += UpdateGivenChange;
                    cashRegister.OnUndo += UndoGivenChange;
                    cashRegister.OnClear += ClearGivenChange;
                    cashRegister.OnConfirm += CheckGivenChange;
                }
                else
                {
                    paymentTerminal.Open();
                    paymentTerminal.OnConfirm += CheckEnteredAmount;
                }
            }
            else
            {
                customerMoney = totalPrice;
            }

            UpdateMonitorText();

            bool isComplete = false;

            // Payment validation logic for cash register
            void CheckGivenChange()
            {
                decimal totalChange = givenChange.Sum(change => change.amount) / 100m;
                isComplete = totalChange >= customerMoney - totalPrice;

                if (isComplete)
                {
                    decimal paymentAmount = customerMoney - totalChange;
                    DataManager.Instance.PlayerMoney += paymentAmount;
                    MissionManager.Instance.UpdateMission(Mission.Goal.Checkout, 1);
                }
                else
                {
                    message.Log("Insufficient change. Please provide the correct amount.", Color.red);
                }
            }

            // Payment validation logic for payment terminal
            void CheckEnteredAmount(decimal amount)
            {
                isComplete = totalPrice == amount;

                if (isComplete)
                {
                    DataManager.Instance.PlayerMoney += amount;
                    MissionManager.Instance.UpdateMission(Mission.Goal.Checkout, 1);
                }
                else
                {
                    message.Log("Invalid amount. Please enter a valid amount.", Color.red);
                }
            }

            if (HasCashier)
            {
                // Auto-complete transaction if there is a cashier
                yield return new WaitForSeconds(1f);
                DataManager.Instance.PlayerMoney += totalPrice;
                isComplete = true;
            }
            else
            {
                yield return new WaitUntil(() => isComplete);
                AudioManager.Instance.PlaySFX(AudioID.Kaching);
            }

            // Finalize transaction
            currentCustomer.Inventory.Clear();
            currentCustomer = null;
            totalPrice = 0m;
            customerMoney = 0m;

            // Clean up UI and close manual payment interfaces
            if (!HasCashier)
            {
                if (isUsingCash)
                {
                    cashRegister.Close();
                    cashRegister.OnDraw -= UpdateGivenChange;
                    cashRegister.OnUndo -= UndoGivenChange;
                    cashRegister.OnClear -= ClearGivenChange;
                    cashRegister.OnConfirm -= CheckGivenChange;
                }
                else
                {
                    paymentTerminal.Close();
                    paymentTerminal.OnConfirm -= CheckEnteredAmount;
                }

                ActivateReturnButton();

                var endPosition = transform.TransformPoint(checkoutPoint + Vector3.up * 1.2f);
                yield return ClearGivenChangeCoroutine(endPosition);
            }

            SetCurrentState(State.Standby);
        }

        private IEnumerator ShowAd()
        {
            yield return new WaitForSeconds(1);
            AdManager.instance.ShowInterstitialAd();
        }

        private void UpdateGivenChange(int amount)
        {
            decimal playerBalance = DataManager.Instance.PlayerMoney + customerMoney;
            decimal totalChange = givenChange.Sum(change => change.amount) / 100m;

            if (playerBalance < totalChange + (amount / 100m)) return;

            givenChange.Push(new ChangeMoney(amount, SpawnMoney(amount)));

            UpdateMonitorText();
        }

        private void UndoGivenChange()
        {
            if (givenChange.Count == 0) return;

            var change = givenChange.Pop();

            var endPosition = cashierPoint.position + Vector3.up;
            change.money.transform.DOMove(endPosition, 0.3f)
                .OnComplete(() => Destroy(change.money));

            UpdateMonitorText();
        }


        private void ClearGivenChange()
        {
            var endPosition = cashierPoint.position + Vector3.up;
            StartCoroutine(ClearGivenChangeCoroutine(endPosition));
        }

        private GameObject SpawnMoney(int amount)
        {
            // Find the corresponding sprite for the given denomination
            int index = denominations.IndexOf(amount);
            var sprite = moneySprites[index];

            // Create a new GameObject to represent the money
            var money = new GameObject("Money_" + amount.ToString());

            // Set initial position at the cashier's position with an upward offset
            money.transform.position = cashierPoint.position + Vector3.up;

            // Apply a random rotation around the Y-axis for a natural scattered look
            money.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);

            // Set a small scale for the money object
            money.transform.localScale = Vector3.one * 0.05f;

            // Add a SpriteRenderer component and assign the corresponding money sprite
            var moneyRend = money.AddComponent<SpriteRenderer>();
            moneyRend.sprite = sprite;

            // Set the rendering order to ensure correct layering
            moneyRend.sortingOrder = givenChange.Count;

            // Determine the final target position, slightly randomized around the central money point
            var center = transform.TransformPoint(moneyPoint);
            var position = center + Random.insideUnitSphere.Flatten() * 0.15f;

            // Move the money to its final position with a smooth animation
            money.transform.DOMove(position, 0.3f);

            return money;
        }

        private IEnumerator ClearGivenChangeCoroutine(Vector3 endPosition, float duration = 0.3f)
        {
            var changeToClear = new List<ChangeMoney>(givenChange);
            givenChange.Clear();
            UpdateMonitorText();

            changeToClear.ForEach(change =>
            {
                change.money.transform.DOMove(endPosition, duration)
                    .OnComplete(() => Destroy(change.money));
            });

            yield return new WaitForSeconds(duration);
        }

/*
        private IEnumerator ClearMoneyRenderers(Vector3 endPosition, float duration = 0.3f)
        {
            moneyRenderers.ForEach(money =>
            {
                money.transform.DOMove(endPosition, duration)
                    .OnComplete(() => Destroy(money.gameObject));
            });

            moneyRenderers.Clear();

            yield return new WaitForSeconds(duration);
        }
*/
        private void SetCurrentState(State state)
        {
            CurrentState = state;
            UpdateMonitorText();
        }

        private void UpdateMonitorText()
        {
            string displayText = "";
            string text="";

            switch (CurrentState)
            {
                case State.Standby:
                    //text = LanguageControl.CheckLanguage("Beklemede...", "Standby...");
                    text = LanguageManager.instance.GetLocalizedValue("PendingStatusText");
                    displayText = $"\n\n{text}";
                    break;

                case State.Placing:
                    //displayText = LanguageControl.CheckLanguage("\n\nBekleniyor...","\n\nWaiting...");
                    displayText = LanguageManager.instance.GetLocalizedValue("WaitingMessageText").Replace("\\n", "\n");
                    break;

                case State.Scanning:
                    displayText = LanguageManager.instance.GetLocalizedValue("ScanningStatusText");
                    text = LanguageManager.instance.GetLocalizedValue("TotalText");
                    displayText += $"\n<color=#00a4ff>{text} ${totalPrice:N2}</color>";
                    break;

                case State.CashPay:
                    displayText = "Cash Payment";
                    displayText += $"\n<color=#00a4ff>Total: ${totalPrice:N2}</color>";
                    displayText += $"\nReceived: ${customerMoney:N2}";

                    decimal changeNeeded = customerMoney - totalPrice;
                    displayText += $"\n<color=yellow>Change: ${changeNeeded:N2}";

                    decimal totalChange = givenChange.Sum(change => change.amount) / 100m;
                    string color = totalChange >= changeNeeded ? "green" : "red";
                    displayText += $"\n<color={color}>Give: ${totalChange:N2}";
                    break;

                case State.CardPay:
                    displayText = LanguageManager.instance.GetLocalizedValue("CardPaymentText");
                    displayText += $"\n<color=#00a4ff>Total: ${totalPrice:N2}</color>";
                    text = LanguageManager.instance.GetLocalizedValue("EnterTotalOnTerminalText").Replace("\\n","\n");
                    displayText += text;
                    break;

                default:
                    break;
            }

            monitorText.text = displayText;
        }

        private void ActivateReturnButton()
        {
            UIManager.Instance.ToggleActionUI(ActionType.Return, true, () =>
            {
                cashierCamera.gameObject.SetActive(false);

                UIManager.Instance.ToggleActionUI(ActionType.Return, false, null);

                player.StateManager.PopState();
                player = null;
            });
        }

        #region Cash Payment Methods
        private decimal GetRandomPaymentOption()
        {
            List<decimal> paymentOptions = new List<decimal>();

            int totalCents = Mathf.RoundToInt((float)totalPrice * 100);

            // 1. Exact payment (if possible)
            if (HasExactChange(totalCents))
            {
                paymentOptions.Add(totalPrice);
            }

            // 2. Round up to nearest convenient denomination
            int roundedUpAmount = GetRoundedUpAmount(totalCents);
            paymentOptions.Add(roundedUpAmount / 100m);

            // 3. Smallest excess payment
            int smallestExcessAmount = GetSmallestExcessAmount(totalCents);
            paymentOptions.Add(smallestExcessAmount / 100m);

            // 4. Higher denomination payment (e.g., round up to nearest 5 or 10 dollar increment)
            int roundedUpHigherDenomination = GetHigherDenomination(totalCents);
            paymentOptions.Add(roundedUpHigherDenomination / 100m);

            int randomIndex = Random.Range(0, paymentOptions.Count);
            return paymentOptions[randomIndex];
        }

        private bool HasExactChange(int amount)
        {
            // Assume exact change is possible if denominations cover it,
            // but make it less likely to be selected (e.g., 30% chance).
            bool hasExact = amount <= denominations[0];
            return hasExact && Random.value < 0.3f;
        }

        private int GetRoundedUpAmount(int amount)
        {
            foreach (int denom in denominations)
            {
                if (amount <= denom)
                {
                    return denom;
                }
            }
            return amount;
        }

        private int GetSmallestExcessAmount(int amount)
        {
            foreach (int denom in denominations)
            {
                if (amount < denom)
                {
                    return denom;
                }
            }
            return amount;
        }

        private int GetHigherDenomination(int amount)
        {
            int nearestHigher = Mathf.CeilToInt(amount / 500f) * 500; // rounding up to nearest $5 increment
            return nearestHigher > amount ? nearestHigher : amount;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Vector3 worldCheckoutPoint = transform.TransformPoint(checkoutPoint);
            Gizmos.DrawWireSphere(worldCheckoutPoint, 0.2f);
            DrawArrow.ForGizmo(worldCheckoutPoint, liningDirection * 3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.TransformPoint(packingPoint), 0.2f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(moneyPoint), 0.15f);

            Vector3 worldCenter = transform.TransformPoint(placementBounds.center);
            Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, placementBounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}
