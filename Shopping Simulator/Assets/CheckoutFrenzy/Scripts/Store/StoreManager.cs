﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using System;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }

        [SerializeField, Tooltip("The 3D bounding box of the store.")]
        private Bounds storeBounds;

        [SerializeField, Tooltip("The checkout counter where customers can pay for their items.")]
        private CheckoutCounter checkoutCounter;

        [SerializeField, Tooltip("The point where deliveries are made (e.g., Products, Furnitures).")]
        private Transform deliveryPoint;

        [SerializeField, Tooltip("The points where customers spawn then walk into the store.")]
        private List<Transform> spawnPoints;

        [SerializeField, Tooltip("The prefabs of the different customer types that can spawn.")]
        private List<Customer> customerPrefabs;

        [SerializeField, Tooltip("The list of available expansions for the store.")]
        private List<Expansion> expansions;

        public Transform DeliveryPoint => deliveryPoint;
        public List<Expansion> Expansions => expansions;

        public event System.Action<License> OnLicensePurchased;
        public event System.Action<int> OnExpansionPurchased;

        public bool IsUIBlockingActions { get; set; }

        private NavMeshSurface navMeshSurface;
        public void UpdateNavMeshSurface() => navMeshSurface.BuildNavMesh();

        // List of valid shelving units within the store's boundaries where customers can interact with products.
        private HashSet<ShelvingUnit> shelvingUnits = new HashSet<ShelvingUnit>();
        public void RegisterShelvingUnit(ShelvingUnit shelvingUnit) => shelvingUnits.Add(shelvingUnit);
        public void UnregisterShelvingUnit(ShelvingUnit shelvingUnit) => shelvingUnits.Remove(shelvingUnit);

        private AudioClip backgroundMusic;

        /// <summary>
        /// Calculates the maximum number of customers that can be in the store at the same time. 
        /// This value is determined by the base maximum number of customers 
        /// plus the sum of additional customers provided by purchased expansions.
        /// </summary>
        private int maxCustomers => GameConfig.Instance.BaseMaxCustomers + expansions
            .Take(DataManager.Instance.Data.ExpansionLevel)
            .Sum(expansion => expansion.AdditionalCustomers);

        private Coroutine spawnCustomerCoroutine;
        private List<Customer> customers = new List<Customer>();
        private List<Customer> liningCustomers = new List<Customer>();

        private TimeRange openTime; // The time range during which the store is open for business.
        private bool isOpen;        // Indicates whether the store is currently open for business (using store sign).          
        private bool isTodayEnded;  // Indicates whether the current in-game day has ended (business time end hour).
        private int lastTotalDays;  // Stores the total number of days that have passed since the game started.

        private void Awake()
        {
            Instance = this;

            navMeshSurface = GetComponent<NavMeshSurface>();

            // Set the target frame rate to 60 frames per second.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            backgroundMusic = GameConfig.Instance.BackgroundMusic;
            openTime = GameConfig.Instance.OpenTime;
        }

        private IEnumerator Start()
        {
            // Wait until the DataManager is initialized and its data is loaded.
            yield return new WaitUntil(() =>
                DataManager.Instance != null && DataManager.Instance.IsLoaded
            );

            DataManager.Instance.OnSave += () =>
            {
                // Calculate and store the total value of products currently held by customers but not yet paid for.
                decimal productsValue = 0;
                foreach (var customer in customers)
                {
                    foreach (var product in customer.Inventory)
                    {
                        decimal price = DataManager.Instance.GetCustomProductPrice(product);
                        productsValue += price;
                    }
                }

                DataManager.Instance.Data.UnpaidProductsValue = productsValue;
            };

            // Restore purchased Expansions
            for (int i = 0; i < expansions.Count; i++)
            {
                bool isPurchased = i < DataManager.Instance.Data.ExpansionLevel;
                expansions[i].SetPurchasedState(isPurchased);
            }

            // Wait a bit to ensure that all scene components have been initialized.
            yield return new WaitForEndOfFrame();

            UpdateNavMeshSurface();

            if (IsOpenTime())
            {
                spawnCustomerCoroutine = StartCoroutine(SpawnCustomer());

                AudioManager.Instance.PlayBGM(backgroundMusic);
            }
        }

        private void Update()
        {
            if (!isTodayEnded && !IsOpenTime())
            {
                StartCoroutine(EndDay());
            }
        }

        private bool IsOpenTime()
        {
            int totalMinutes = TimeManager.Instance.TotalMinutes;
            return openTime.IsWithinRange(totalMinutes);
        }

        private IEnumerator SpawnCustomer()
        {       
            while (true)
            {
                float waitTime = GameConfig.Instance.GetRandomSpawnTime2;        
                yield return new WaitForSeconds(waitTime);
              
                if (isOpen && customers.Count < maxCustomers && shelvingUnits.Count > 0)
                {
                    int randomCustomerIndex =UnityEngine.Random.Range(0, customerPrefabs.Count);
                    var customerPrefab = customerPrefabs[randomCustomerIndex];

                    int randomSpawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                    var spawnPoint = spawnPoints[randomSpawnIndex];

                    var customer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
                    customers.Add(customer);
             
                    DataManager.Instance.Data.CurrentSummary.TotalCustomers++;
                }
            }
        }

        /// <summary>
        /// Gets the queue number, position, and look direction for the given customer at the checkout counter.
        /// </summary>
        /// <param name="customer">The customer for whom to get the queue information.</param>
        /// <returns>A tuple containing the queue number, queue position, and look direction for the customer.</returns>
        public (int queueNumber, Vector3 queuePosition, Vector3 lookDirection) GetQueueNumber(Customer customer)
        {
            if (!liningCustomers.Contains(customer))
            {
                liningCustomers.Add(customer);
            }

            int number = liningCustomers.IndexOf(customer);
            Vector3 position = checkoutCounter.GetQueuePosition(number, out Vector3 direction);

            return (number, position, direction);
        }

        /// <summary>
        /// Handles the checkout process for the given customer.
        /// This coroutine simulates the customer placing their products on the counter and waiting for them to be processed.
        /// </summary>
        /// <param name="customer">The customer to be checked out.</param>
        /// <returns>An IEnumerator to control the checkout process.</returns>
        public IEnumerator Checkout(Customer customer)
        {
            yield return checkoutCounter.PlaceProducts(customer);

            yield return new WaitUntil(() => checkoutCounter.CurrentState == CheckoutCounter.State.Standby);

            yield return CustomerLeave(customer);
        }

        /// <summary>
        /// Handles the customer leaving the store after completing their shopping.
        /// This method guides the customer to an exit point and then destroys their game object.
        /// </summary>
        /// <param name="customer">The customer to be removed from the store.</param>
        /// <returns>An IEnumerator to control the customer's leaving process.</returns>
        public IEnumerator CustomerLeave(Customer customer)
        {
            print("Müşteri dükkandan ayrılıyor...");

            if (customer.waitingTimeExceeding)
            {
                ReputationManager.instance.RegisterCustomerFeedback(false);
                //string language = PlayerPrefs.GetString("Language");
                //string chat = language == "English" ? GameConfig.Instance.WaitingLongDialogueEnglish.GetRandomLine() 
                //    : GameConfig.Instance.WaitingLongDialogueTurkish.GetRandomLine();

                int index = LanguageManager.GetCurrentLanguageIndex();
                string chat = GameConfig.Instance.WaitingLongDialogues[index].GetRandomLine();

                customer.UpdateChatBubble(chat);
            }

            liningCustomers.Remove(customer);
            customers.Remove(customer);

            var exitPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position;
            yield return customer.MoveTo(exitPoint);
            Destroy(customer.gameObject);
        }

        /// <summary>
        /// Validates the position of a ShelvingUnit. 
        /// 
        /// If the ShelvingUnit is within the store bounds, it is registered; 
        /// otherwise, it is unregistered.
        /// </summary>
        /// <param name="shelvingUnit">The ShelvingUnit to validate.</param>
        public void ValidateShelvingUnit(ShelvingUnit shelvingUnit)
        {
            Vector3 position = shelvingUnit.transform.position;

            if (IsWithinStore(position)) RegisterShelvingUnit(shelvingUnit);
            else UnregisterShelvingUnit(shelvingUnit);
        }

        /// <summary>
        /// Gets a random ShelvingUnit from the list of registered shelving units.
        /// </summary>
        /// <returns>A randomly selected ShelvingUnit, or null if no shelving units are registered.</returns>
        public ShelvingUnit GetShelvingUnit()
        {
            if (shelvingUnits.Count == 0) return null;

            int randomIndex = UnityEngine.Random.Range(0, shelvingUnits.Count);
            var shelvingUnit = shelvingUnits.ElementAt(randomIndex);

            return shelvingUnit;
        }

        /// <summary>
        /// Checks if the given position is within the bounds of the store.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is within the store bounds, otherwise false.</returns>
        public bool IsWithinStore(Vector3 position)
        {
            return storeBounds.Contains(position);
        }

        /// <summary>
        /// Attempts to purchase the specified license.
        /// 
        /// Checks if the player has enough money. If so, deducts the license price from the player's money, 
        /// grants the license to the player, and updates the game state accordingly.
        /// </summary>
        /// <param name="license">The license to be purchased.</param>
        /// <returns>True if the license was purchased successfully, otherwise false.</returns>
        public bool PurchaseLicense(License license)
        {
            if (DataManager.Instance.PlayerMoney < license.Price)
            {
                string text = LanguageManager.instance.GetLocalizedValue("NotEnoughMoneyText");
                UIManager.Instance.Message.Log(text, Color.red);
                return false;
            }

            // Grants the purchased license
            foreach (var product in license.Products)
            {
                DataManager.Instance.Data.LicensedProducts.Add(product.ProductID);
            }

            DataManager.Instance.PlayerMoney -= license.Price;

            OnLicensePurchased?.Invoke(license);

            AudioManager.Instance.PlaySFX(AudioID.Kaching);

            return true;
        }

        /// <summary>
        /// Checks if the specified expansion has been purchased.
        /// </summary>
        /// <param name="expansion">The expansion to check.</param>
        /// <returns>True if the expansion has been purchased, otherwise false.</returns>
        public bool IsExpansionPurchased(Expansion expansion)
        {
            int expansionIndex = expansions.IndexOf(expansion);
            return expansionIndex < DataManager.Instance.Data.ExpansionLevel;
        }

        /// <summary>
        /// Checks if the specified expansion is the currently active expansion.
        /// </summary>
        /// <param name="expansion">The expansion to check.</param>
        /// <returns>True if the expansion is the current expansion, otherwise false.</returns>
        public bool IsCurrentExpansion(Expansion expansion)
        {
            int expansionIndex = expansions.IndexOf(expansion);
            return expansionIndex == DataManager.Instance.Data.ExpansionLevel;
        }

        /// <summary>
        /// Attempts to purchase the next available expansion.
        /// 
        /// Checks if the player has enough money. If so, deducts the expansion price, 
        /// updates the expansion level, and updates the game state accordingly.
        /// </summary>
        /// <returns>True if the expansion was purchased successfully, otherwise false.</returns>
        public bool PurchaseExpansion()
        {
            var expansion = expansions[DataManager.Instance.Data.ExpansionLevel];

            if (DataManager.Instance.PlayerMoney < expansion.UnlockPrice)
            {
                string text = LanguageManager.instance.GetLocalizedValue("NotEnoughMoneyText");
                UIManager.Instance.Message.Log(text, Color.red);
                return false;
            }

            expansion.SetPurchasedState(true);
            DataManager.Instance.Data.ExpansionLevel++;
            DataManager.Instance.PlayerMoney -= expansion.UnlockPrice;

            UpdateNavMeshSurface();

            OnExpansionPurchased?.Invoke(DataManager.Instance.Data.ExpansionLevel);

            AudioManager.Instance.PlaySFX(AudioID.Kaching);
            AudioManager.Instance.PlaySFX(AudioID.Construction);

            return true;
        }

        private IEnumerator EndDay()
        {
            isTodayEnded = true;
            lastTotalDays = DataManager.Instance.Data.TotalDays;

            TimeManager.Instance.AllowTimeUpdate = false;

            if (spawnCustomerCoroutine != null)
            {
                StopCoroutine(spawnCustomerCoroutine);
                spawnCustomerCoroutine = null;
            }

            AskCustomersToLeave();

            yield return new WaitWhile(() => customers.Count > 0);

            if (checkoutCounter.HasCashier) checkoutCounter.HasCashier = false;

            IsUIBlockingActions = true;

            UIManager.Instance.SummaryScreen.Show(DataManager.Instance.Data.CurrentSummary, (skip) =>
            {
                if (skip) SkipToNextDay();
                else StartCoroutine(ShowSkipDialog());

                TimeManager.Instance.AllowTimeUpdate = true;
                IsUIBlockingActions = false;
            });

            AudioManager.Instance.StopBGM();
        }

        private void SkipToNextDay()
        {
            if (lastTotalDays == DataManager.Instance.Data.TotalDays) DataManager.Instance.Data.TotalDays++;

            int hour = openTime.StartHour;
            int minute = openTime.StartMinute;

            TimeManager.Instance.SetTime(hour, minute);

            RestartDay();
        }

        private IEnumerator ShowSkipDialog()
        {
            bool skipConfirmed = false;

            UIManager.Instance.SkipDialog.Show(() =>
            {
                skipConfirmed = true;
            });

            yield return new WaitUntil(() => skipConfirmed || IsOpenTime());

            if (skipConfirmed)
            {
                SkipToNextDay();
                yield break;
            }

            UIManager.Instance.SkipDialog.Hide();

            RestartDay();
        }

        private void RestartDay()
        {
            isTodayEnded = false;
            DataManager.Instance.Data.CurrentSummary = new SummaryData(DataManager.Instance.PlayerMoney);
            spawnCustomerCoroutine = StartCoroutine(SpawnCustomer());

            AudioManager.Instance.PlayBGM(backgroundMusic);
        }

        /// <summary>
        /// Toggles the open/closed state of the store.
        /// 
        /// If the store is opened, customers can enter and shop. 
        /// If the store is closed, customers are asked to leave.
        /// </summary>
        /// <param name="isOpen">Whether to open or close the store.</param>
        public void ToggleOpenState(bool isOpen)
        {
            this.isOpen = isOpen;
            if (!isOpen) AskCustomersToLeave();
        }

        private void AskCustomersToLeave()
        {
            var leavingCustomers = new List<Customer>(customers);
            leavingCustomers.ForEach(customer => customer.AskToLeave());
        }

        /// <summary>
        /// Hires a cashier for the checkout counter.
        /// 
        /// Checks if a cashier is already hired and if the player has enough money. 
        /// Deducts the cashier cost from the player's money and updates the checkout counter's state.
        /// </summary>
        public void HireCashier()
        {
            if (checkoutCounter.HasCashier)
            {
                string text2 = LanguageManager.instance.GetLocalizedValue("CashierAlreadyHiredText");
                UIManager.Instance.Message.Log(text2);
                return;
            }

            int cashierCost = GameConfig.Instance.CashierCost;

            if (DataManager.Instance.PlayerMoney < cashierCost)
            {
                string text1 = LanguageManager.instance.GetLocalizedValue("NotEnoughMoneyText");
                UIManager.Instance.Message.Log(text1, Color.red);
                return;
            }

            DataManager.Instance.PlayerMoney -= cashierCost;
            checkoutCounter.HasCashier = true;
            string text = LanguageManager.instance.GetLocalizedValue("CashierHiredSuccessText");
            UIManager.Instance.Message.Log(text);
            AudioManager.Instance.PlaySFX(AudioID.Kaching);
        }

        /// <summary>
        /// Hires a cleaner to remove all empty boxes from the store.
        /// 
        /// Checks if there are any empty boxes in the store and if the player has enough money. 
        /// Deducts the cleaner cost from the player's money and destroys all empty boxes.
        /// </summary>
        public void HireCleaner()
        {
            var emptyBoxes = FindObjectsOfType<Box>()
                .Where(box => box.Quantity == 0)
                .ToList();

            if (emptyBoxes.Count == 0)
            {
                string text = LanguageManager.instance.GetLocalizedValue("StoreAlreadyCleanText");
                UIManager.Instance.Message.Log(text);
                return;
            }

            int cleanerCost = GameConfig.Instance.CleanerCost;

            if (DataManager.Instance.PlayerMoney < cleanerCost)
            {
                string text2 = LanguageManager.instance.GetLocalizedValue("NotEnoughMoneyText");
                UIManager.Instance.Message.Log(text2, Color.red);
                return;
            }

            DataManager.Instance.PlayerMoney -= cleanerCost;
            emptyBoxes.ForEach(box => Destroy(box.gameObject));
            string text3 =LanguageManager.instance.GetLocalizedValue("CleanerHiredSuccessText");
            UIManager.Instance.Message.Log(text3);
            AudioManager.Instance.PlaySFX(AudioID.Kaching);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(storeBounds.center, storeBounds.size);
        }
#endif
    }
}
