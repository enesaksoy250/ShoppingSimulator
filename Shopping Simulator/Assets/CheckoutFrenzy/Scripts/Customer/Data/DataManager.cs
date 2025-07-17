using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace CryingSnow.CheckoutFrenzy
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        public List<Product> ProductDB { get; private set; }
        public List<Furniture> FurnitureDB { get; private set; }
        public List<License> LicenseDB { get; private set; }

        public List<LoanTemplate> LoanTemplateDB { get; private set; }
        public GameData Data { get; private set; }

        public Coroutine startAdProcessCoroutine;
 
        public decimal PlayerMoney
        {
            get => Data.PlayerMoney;
            set
            {
                decimal previousValue = Data.PlayerMoney;
                Data.PlayerMoney = value;
                OnMoneyChanged?.Invoke(value);

                if (value > previousValue)
                {
                    // Money increase, adding to revenues
                    Data.CurrentSummary.TotalRevenues += value - previousValue;
                }
                else if (value < previousValue)
                {
                    // Money decrease, adding to spending
                    Data.CurrentSummary.TotalSpending += previousValue - value;
                }
                if (Data.PlayerMoney < 200)
                {
                    if (startAdProcessCoroutine == null)
                        startAdProcessCoroutine = StartCoroutine(AdManager.instance.StartInterstitialRewardedAtProcess());
                }
            }
        }

        public event System.Action OnSave;
        public event System.Action<decimal> OnMoneyChanged;
        public event System.Action<int> OnPriceChanged;
        public event System.Action<float> OnExperienceGain;
        public event System.Action<int> OnLevelUp;

        public bool IsLoaded { get; private set; }

        private System.DateTime sessionStartTime;

        private FurnitureBox furnitureBoxPrefab;


        private void Awake()
        {
            Instance = this;

            LoadDatabases();
            LoadGameData();

            IsLoaded = true;
            sessionStartTime = System.DateTime.Now;
        }

        private void Start()
        {
            StartCoroutine(AutoSaveSystem());
       
            AdManager.instance.OnAdCoroutineFinished += () => { startAdProcessCoroutine = null; };

            DatabaseManager.Instance.OnGameDataLoaded += HandleFirstRunFirebaseLoadSuccess;
        }

        IEnumerator AddEx()
        {
            for (int i = 0; i < 50; i++)
            {
                print("Bu " + (i + 1));
                AddExperience(100);
                yield return new WaitForSeconds(.1f); // 1 saniye bekle
            }
        }

        private void OnApplicationQuit()
        {
            DOTween.KillAll();
            SaveGameData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveGameData();
        }


        private void HandleFirstRunFirebaseLoadSuccess(GameData loadedData)
        {
         
            if (DatabaseManager.Instance != null)
            {
                DatabaseManager.Instance.OnGameDataLoaded -= HandleFirstRunFirebaseLoadSuccess;              
            }

            if (loadedData != null)
            {
                Data = loadedData;
                Debug.Log("GameData loaded from Firebase. Saving a copy to local storage.");
                //StartCoroutine(CloseLoadingPanel(2));
                ApplyLoadedData();
                SaveGameDataLocally();
            
            }
            else
            {
                GamePanelManager.instance.LoadPanel("ErrorPanel");
            }


        }



        private void ApplyLoadedData()
        {
            if (Data == null) return;
            Debug.Log("Applying loaded game data...");

            // Önceki objeleri temizle
            foreach (var furniture in FindObjectsOfType<Furniture>()) Destroy(furniture.gameObject);
            foreach (var box in FindObjectsOfType<Box>()) Destroy(box.gameObject);

            // 1. Furniture'ları yükle
            if (Data.SavedFurnitures != null && Data.SavedFurnitures.Count > 0)
            {
                foreach (var savedFurniture in Data.SavedFurnitures)
                {
                    Vector3 lastMoved = savedFurniture.LastMoved.ToVector3();

                    if (lastMoved != Vector3.zero)
                    {
                        var furnitureBox = Instantiate(furnitureBoxPrefab, lastMoved, Quaternion.identity);
                        furnitureBox.furnitureId = savedFurniture.FurnitureID;
                        continue;
                    }

                    var furniturePrefab = GetFurnitureById(savedFurniture.FurnitureID);
                    if (furniturePrefab == null) continue;

                    Vector3 position = savedFurniture.Location.ToVector3();
                    Quaternion rotation = savedFurniture.Orientation.ToQuaternion();
                    var furniture = Instantiate(furniturePrefab, position, rotation);

                    if (furniture is ShelvingUnit shelvingUnit)
                    {
                        shelvingUnit.RestoreProductsOnShelves(savedFurniture.SavedShelves);
                    }
                    else if (furniture is StorageRack storageRack)
                    {
                        storageRack.RestoreBoxesOnRacks(savedFurniture.SavedRacks);
                    }
                }
            }

            // 2. Kutuları yükle
            if (Data.SavedBoxes != null && Data.SavedBoxes.Count > 0)
            {
                Dictionary<string, Box> boxPrefabs = Resources.LoadAll<Box>("Boxes").ToDictionary(box => box.name);
                foreach (var savedBox in Data.SavedBoxes)
                {
                    if (!boxPrefabs.TryGetValue(savedBox.Name, out var boxPrefab)) continue;

                    Vector3 position = savedBox.Location.ToVector3();
                    Quaternion rotation = savedBox.Orientation.ToQuaternion();
                    var box = Instantiate(boxPrefab, position, rotation);
                    box.name = boxPrefab.name;

                    if (!savedBox.IsEmpty)
                    {
                        var product = GetProductById(savedBox.ProductID);
                        int quantity = savedBox.Quantity;
                        box.RestoreProducts(product, quantity);
                    }

                    if (savedBox.IsOpen) box.SetLidsOpen();
                }
            }

            // 3. FurnitureBox'ları yükle
            if (Data.SavedFurnitureBoxes != null && Data.SavedFurnitureBoxes.Count > 0)
            {
                foreach (var savedFurnitureBox in Data.SavedFurnitureBoxes)
                {
                    Vector3 position = savedFurnitureBox.Location.ToVector3();
                    Quaternion rotation = savedFurnitureBox.Orientation.ToQuaternion();
                    var furnitureBox = Instantiate(furnitureBoxPrefab, position, rotation);
                    furnitureBox.furnitureId = savedFurnitureBox.FurnitureID;
                }
            }

            // 4. Oyuncu parasını güncelle
            PlayerMoney += Data.PendingOrdersValue;
            Data.PendingOrdersValue = 0m;

            PlayerMoney += Data.UnpaidProductsValue;
            Data.UnpaidProductsValue = 0m;

            // 5. Cleanable objeleri yükle
            if (Data.SavedCleanables != null && Data.SavedCleanables.Count > 0)
            {
                Dictionary<int, Cleanable> cleanables = Resources.LoadAll<Cleanable>("Cleanables")
                      .ToDictionary(c => c.CleanableID);

                foreach (var cleanableData in Data.SavedCleanables)
                {
                    if (!cleanables.TryGetValue(cleanableData.CleanableID, out var cleanablePrefab)) continue;

                    Vector3 position = cleanableData.Location.ToVector3();
                    Quaternion rotation = cleanableData.Orientation.ToQuaternion();
                    var cleanable = Instantiate(cleanablePrefab, position, rotation);
                }
            }

            Debug.Log("Game state application complete.");
            // Oyun durumu uygulandıktan sonra tetiklenecek event buraya eklenebilir
        }


        private void SaveGameDataLocally()
        {

            Data.SavedFurnitures.Clear();
            Data.SavedBoxes.Clear();

            Data.SavedFurnitureBoxes.Clear();
            Data.SavedCleanables.Clear();

            Data.TotalMinutes = TimeManager.Instance.TotalMinutes;

            OnSave?.Invoke();

            SaveSystem.SaveData<GameData>(Data, "GameData");

            ReputationManager.instance.SaveReputation();

        }

        private void LoadDatabases()
        {
            ProductDB = Resources.LoadAll<Product>("Products").OrderBy(p => p.ProductID).ToList();
            FurnitureDB = Resources.LoadAll<Furniture>("Furnitures").OrderBy(f => f.FurnitureID).ToList();
            LicenseDB = Resources.LoadAll<License>("Licenses").ToList();
            LoanTemplateDB = Resources.LoadAll<LoanTemplate>("LoanTemplates").OrderBy(l => l.Principal).ToList();
            furnitureBoxPrefab = Resources.Load<FurnitureBox>("Boxes/FurnitureBox");
        }

        private void LoadGameData()
        {
            Data = SaveSystem.LoadData<GameData>("GameData");

            if (Data != null)
            {

                if (Data.HiredEmployees == null)
                    Data.HiredEmployees = new List<EmployeeData>();

                if (Data.Bills == null)
                    Data.Bills = new List<Bill>();

                if (Data.Loans == null)
                    Data.Loans = new List<Loan>(); 

                // 1. Restore Saved Furniture Pieces
                foreach (var furniture in FindObjectsOfType<Furniture>())
                {
                    Destroy(furniture.gameObject);
                }

                foreach (var savedFurniture in Data.SavedFurnitures)
                {
                    Vector3 lastMoved = savedFurniture.LastMoved.ToVector3();

                    if (lastMoved != Vector3.zero)
                    {
                        var furnitureBox = Instantiate(furnitureBoxPrefab, lastMoved, Quaternion.identity);
                        furnitureBox.furnitureId = savedFurniture.FurnitureID;
                        continue;
                    }

                    var furniturePrefab = GetFurnitureById(savedFurniture.FurnitureID);

                    Vector3 position = savedFurniture.Location.ToVector3();
                    Quaternion rotation = savedFurniture.Orientation.ToQuaternion();

                    var furniture = Instantiate(furniturePrefab, position, rotation);

                    if (furniture is ShelvingUnit shelvingUnit)
                    {
                        shelvingUnit.RestoreProductsOnShelves(savedFurniture.SavedShelves);
                    }
                    else if (furniture is StorageRack storageRack)
                    {
                        storageRack.RestoreBoxesOnRacks(savedFurniture.SavedRacks);
                    }
                }

                // 2. Restore Saved Boxes
                Dictionary<string, Box> boxPrefabs = Resources.LoadAll<Box>("Boxes")
                    .ToDictionary(box => box.name);

                foreach (var savedBox in Data.SavedBoxes)
                {
                    var boxPrefab = boxPrefabs[savedBox.Name];
                    Vector3 position = savedBox.Location.ToVector3();
                    Quaternion rotation = savedBox.Orientation.ToQuaternion();
                    var box = Instantiate(boxPrefab, position, rotation);
                    box.name = boxPrefab.name;

                    if (!savedBox.IsEmpty)
                    {
                        var product = GetProductById(savedBox.ProductID);
                        int quantity = savedBox.Quantity;
                        box.RestoreProducts(product, quantity);
                    }

                    if (savedBox.IsOpen) box.SetLidsOpen();
                }

                if (Data.SavedFurnitureBoxes == null)
                    Data.SavedFurnitureBoxes = new();

                // 3. Restore Saved Furniture Boxes
                foreach (var savedFurnitureBox in Data.SavedFurnitureBoxes)
                {
                    Vector3 position = savedFurnitureBox.Location.ToVector3();
                    Quaternion rotation = savedFurnitureBox.Orientation.ToQuaternion();
                    var furnitureBox = Instantiate(furnitureBoxPrefab, position, rotation);
                    furnitureBox.furnitureId = savedFurnitureBox.FurnitureID;
                }

                // 4. Refund pending orders from PC to Player's Money
                PlayerMoney += Data.PendingOrdersValue;
                Data.PendingOrdersValue = 0m;

                // 5. Refund unpaid products customers were carrying to Player's money
                PlayerMoney += Data.UnpaidProductsValue;
                Data.UnpaidProductsValue = 0m;

                // 6. Restore saved cleanables
                Dictionary<int, Cleanable> cleanables = Resources.LoadAll<Cleanable>("Cleanables")
                    .ToDictionary(c => c.CleanableID);

                if (Data.SavedCleanables == null)
                    Data.SavedCleanables = new();

                foreach (var cleanableData in Data.SavedCleanables)
                {
                    var cleanablePrefab = cleanables[cleanableData.CleanableID];
                    Vector3 position = cleanableData.Location.ToVector3();
                    Quaternion rotation = cleanableData.Orientation.ToQuaternion();
                    var cleanable = Instantiate(cleanablePrefab, position, rotation);
                }
            }
            else
            {
                Data = new GameData();
                Data.Initialize();

                foreach (var license in LicenseDB)
                {
                    if (license.IsOwnedByDefault)
                    {
                        license.Products.ForEach(p => Data.LicensedProducts.Add(p.ProductID));
                    }
                }
            }
        }

        public void SaveGameData()
        {
            SaveGameDataLocally();

            if (PlayerPrefs.HasKey("GoogleLogin"))
            {
                DatabaseManager.Instance.SaveGameDataToFirebase(Data);            
            }
              
        }

        public Product GetProductById(int id)
        {
            return ProductDB.FirstOrDefault(p => p.ProductID == id);
        }

        public Furniture GetFurnitureById(int id)
        {
            return FurnitureDB.FirstOrDefault(f => f.FurnitureID == id);
        }

        /// <summary>
        /// Adds or updates a custom product price.
        /// </summary>
        /// <param name="customPrice">
        /// The custom price to set.
        /// If a custom price already exists for the product, it will be updated.
        /// </param>
        public void AddCustomProductPrice(CustomPrice customPrice) // Renamed for clarity
        {
            var existingCustomPrice = Data.CustomPrices.FirstOrDefault(cp => cp.ProductId == customPrice.ProductId);

            if (existingCustomPrice != null)
            {
                existingCustomPrice.PriceInCents = customPrice.PriceInCents;
            }
            else
            {
                Data.CustomPrices.Add(customPrice);
            }

            OnPriceChanged?.Invoke(customPrice.ProductId);
        }

        /// <summary>
        /// Gets the custom price of a product, or the default price if no custom price is set.
        /// </summary>
        /// <param name="product">The product to get the price for.</param>
        /// <returns>The custom price of the product in dollars, or the default price if no custom price is set.</returns>
        public decimal GetCustomProductPrice(Product product)
        {
            var customPrice = Data.CustomPrices.FirstOrDefault(cp => cp.ProductId == product.ProductID);

            return customPrice?.PriceInCents / 100m ?? product.Price;
        }

        public void AddExperience(int amount = 1)
        {
            Data.CurrentExperience += amount;
            int experienceForNextLevel = CalculateExperienceForNextLevel();

            // Level Up
            while (Data.CurrentExperience >= experienceForNextLevel)
            {
                Data.CurrentExperience -= experienceForNextLevel;
                Data.CurrentLevel++;
                DatabaseManager.Instance.IncreaseFirebaseInfo("level", 1);
                DatabaseManager.Instance.IncreaseLevel();
                OnLevelUp?.Invoke(Data.CurrentLevel);
                string text = LanguageManager.instance.GetLocalizedValue("LevelUpText");
                UIManager.Instance.Message.Log(text, Color.yellow);
                AudioManager.Instance.PlaySFX(AudioID.LevelUp);

                experienceForNextLevel = CalculateExperienceForNextLevel();
            }

            float progress = (float)Data.CurrentExperience / experienceForNextLevel;
            OnExperienceGain?.Invoke(progress);
        }

        public int CalculateExperienceForNextLevel()
        {
            int baseExperience = GameConfig.Instance.BaseExperience;
            float growthFactor = GameConfig.Instance.GrowthFactor;

            return Mathf.CeilToInt(baseExperience * Mathf.Pow(growthFactor, Data.CurrentLevel - 1));
        }

        IEnumerator AutoSaveSystem()
        {     
            while (true)
            {
                yield return new WaitForSeconds(300);
                SaveGameData();
            }
        }

        private IEnumerator CloseLoadingPanel(int time)
        {
            GamePanelManager.instance.LoadPanel("LoadingPanel2");
            yield return new WaitForSeconds(time);
            GamePanelManager.instance.ClosePanel("LoadingPanel2");
        }

        private void Update()
        {
            if (SimpleInput.GetButtonDown("Debug Money") || Input.GetKeyDown(KeyCode.M))
            {
                PlayerMoney += 1000m;
                AudioManager.Instance.PlaySFX(AudioID.Kaching);
                Debug.Log("Added $1,000 to Player's money. REMOVE ON BUILD!");
            }

            if (SimpleInput.GetButtonDown("Debug Experience") || Input.GetKeyDown(KeyCode.L))
            {
                AddExperience(100);
                Debug.Log("Added 100 experience points. REMOVE ON BUILD!");
            }
        }

    }

 }
