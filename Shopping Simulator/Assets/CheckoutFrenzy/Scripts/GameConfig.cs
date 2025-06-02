using System.Collections.Generic;
using UnityEngine;

namespace CryingSnow.CheckoutFrenzy
{
    public class GameConfig : ScriptableObject
    {
        private static GameConfig _instance;

        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("GameConfig");

                    if (_instance == null)
                    {
                        Debug.LogError("GameConfig not found in Resources!\nPlease create one using Tools > Checkout Frenzy > Game Config.");
                    }
                }
                return _instance;
            }
        }

        [Header("Store Settings")]
        [SerializeField, Tooltip("The default name for the store.")]
        private string defaultStoreName = "AWESOME MART";

        [SerializeField, Tooltip("The maximum number of characters allowed for the store name.")]
        private int storeNameMaxCharacters = 15;

        [SerializeField, Tooltip("The initial amount of money the player starts with.")]
        private int startingMoney = 2000;

        [SerializeField, Tooltip("The minimum time (in seconds) between customer spawns.")]
        private float minSpawnTime = 5f;

        [SerializeField, Tooltip("The maximum time (in seconds) between customer spawns.")]
        private float maxSpawnTime = 15f;

        [SerializeField, Tooltip("The base maximum number of customers that can be in the store at the same time.")]
        private int baseMaxCustomers = 5;

        [SerializeField, Tooltip("The time range during which the store is open for business.")]
        private TimeRange openTime;



        [Header("Service Costs")]
        [SerializeField, Tooltip("The cost to hire a cashier.")]
        private int cashierCost = 1000;

        [SerializeField, Tooltip("The cost to hire a cleaner.")]
        private int cleanerCost = 500;



        [Header("Level System")]
        [SerializeField, Tooltip("Base experience required for level 1.")]
        private int baseExperience = 5;

        [SerializeField, Tooltip("Exponential growth factor for experience requirements.")]
        private float growthFactor = 1.1f;



        [Header("Music")]
        [SerializeField, Tooltip("The main Background Music (BGM) for the game.")]
        private AudioClip backgroundMusic;



        [Header("Game Layers")]
        [SerializeField, Tooltip("Layer used for interactable objects (Furnitures, Counter, PC, etc.)")]
        private LayerMask interactableLayer;

        [SerializeField, Tooltip("Layer used for items currently being processed at the checkout counter.")]
        private LayerMask checkoutItemLayer;

        [SerializeField, Tooltip("Layer used for payment method GameObjects (either cash or card).")]
        private LayerMask paymentLayer;

        [SerializeField, Tooltip("Layer used to determine valid placement locations for furniture objects.")]
        private LayerMask groundLayer;

        [SerializeField, Tooltip("Layer used by Shelves in Shelving Units.")]
        private LayerMask shelfLayer;

        [SerializeField, Tooltip("Layer used by objects that can be held by the player. Objects on this layer are rendered on top of everything else using a special camera to prevent clipping when held.")]
        private LayerMask heldObjectLayer;

        [SerializeField, Tooltip("Layer used by the player.")]
        private LayerMask playerLayer;


        [SerializeField] List<Dialogue> notFoundDialogues;
        [SerializeField] List<Dialogue> overpricedDialogues;
        [SerializeField] List<Dialogue> satisfiedDialogues;
        [SerializeField] List<Dialogue> waitingLongDialogues;

        [Header("Control Settings")]
        [SerializeField, Tooltip("Selected control mode for the game.")]
        private ControlMode controlMode;
        
        [Header("Spawn Times")]
        [SerializeField] private float minSpawnTimeAtMinReputation = 15f;
        [SerializeField] private float maxSpawnTimeAtMinReputation = 45f;
        [SerializeField] private float minSpawnTimeAtMaxReputation = 5f;
        [SerializeField] private float maxSpawnTimeAtMaxReputation = 15f;

     

        // Store Settings
        public string DefaultStoreName => defaultStoreName;
        public int StoreNameMaxCharacters => storeNameMaxCharacters;
        public int StartingMoney => startingMoney;
        public float GetRandomSpawnTime => Random.Range(minSpawnTime, maxSpawnTime);
  
        public float GetRandomSpawnTime2
        {
            get
            {
                float reputation = ReputationManager.instance.reputation;
                float t = reputation / 100f;

                float minSpawn = Mathf.Lerp(minSpawnTimeAtMinReputation, minSpawnTimeAtMaxReputation, t);
                float maxSpawn = Mathf.Lerp(maxSpawnTimeAtMinReputation, maxSpawnTimeAtMaxReputation, t);

                return Random.Range(minSpawn, maxSpawn);
            }
        }
        public int BaseMaxCustomers => baseMaxCustomers;
        public TimeRange OpenTime => openTime;

        // Service Cost
        public int CashierCost => cashierCost;
        public int CleanerCost => cleanerCost;

        // Level System
        public int BaseExperience => baseExperience;
        public float GrowthFactor => growthFactor;

        // Music
        public AudioClip BackgroundMusic => backgroundMusic;

        // Game Layers
        public LayerMask InteractableLayer => interactableLayer;
        public LayerMask CheckoutItemLayer => checkoutItemLayer;
        public LayerMask PaymentLayer => paymentLayer;
        public LayerMask GroundLayer => groundLayer;
        public LayerMask ShelfLayer => shelfLayer;
        public LayerMask HeldObjectLayer => heldObjectLayer;
        public LayerMask PlayerLayer => playerLayer;

        public List<Dialogue> NotFoundDialogues => notFoundDialogues;
        public List<Dialogue> OverPricedDialogues => overpricedDialogues;
        public List<Dialogue> SatisfiedDialogues => satisfiedDialogues;
        public List<Dialogue> WaitingLongDialogues => waitingLongDialogues;


        // Control Mode
        public ControlMode ControlMode => controlMode;
    }

    public enum ControlMode { Mobile, PC }
    public enum ActionType { Throw, Open, Close, Place, Take, Price, Rotate, Return }
}
