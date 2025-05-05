using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Customer : MonoBehaviour
    {
        [SerializeField] private HandAttachments handAttachments;

        public List<Product> Inventory => inventory;

        private Animator animator;
        private NavMeshAgent agent;

        private ShelvingUnit shelvingUnit;
        private List<Product> inventory = new List<Product>();
        private int queueNumber = int.MaxValue;

        private bool isPicking;

        private ChatBubble chatBubble;
        private Dialogue notFoundDialogue => GameConfig.Instance.NotFoundDialogue;
        private Dialogue notFoundDialogueTurkish => GameConfig.Instance.NotFoundDialogueTurkish;
        private Dialogue overpricedDialogue => GameConfig.Instance.OverpricedDialogue;
        private Dialogue overPricedDialogueTurkish => GameConfig.Instance.OverPricedDialogueTurkish;
        private Dialogue satisfiedDialogueTurkish => GameConfig.Instance.SatisfiedDialogueTurkish;
        private Dialogue satisfiedDialogueEnglish => GameConfig.Instance.SatisfiedDialogueEnglish;
        private Dialogue waitingLongDialoguEnglish => GameConfig.Instance.WaitingLongDialogueEnglish;
        private Dialogue waitingLongDialoguTurkish => GameConfig.Instance.WaitingLongDialogueTurkish;
        
        private string gameLanguage;
      
        private int waitingTimeAtCheckout;
       
        private int maxWaitingTimeAtCheckout = 63;
        public bool waitingTimeExceeding { get; private set; } = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();

            // Initialize NavMeshAgent parameters
            agent.speed = 1.5f;
            agent.angularSpeed = 3600f;
            agent.acceleration = 100f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        private void Start()
        {
            StartCoroutine(CheckEnteringStore());
            StartCoroutine(Shopping());
            gameLanguage = PlayerPrefs.GetString("Language");
        }

        private void Update()
        {
            CheckStoreDoors();
        }

        // Detecting store's doors and open them if they are closed.
        private void CheckStoreDoors()
        {
            Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1f, GameConfig.Instance.InteractableLayer))
            {
                if (hit.transform.TryGetComponent<EntranceDoor>(out EntranceDoor door))
                {
                    door.OpenIfClosed();
                }
            }
        }

        // Check the first time customer entering store, and ring the bell.
        private IEnumerator CheckEnteringStore()
        {
            while (!StoreManager.Instance.IsWithinStore(transform.position))
            {
                yield return new WaitForSeconds(0.1f);
            }

            AudioManager.Instance.PlaySFX(AudioID.Bell);
        }

        private IEnumerator Shopping()
        {
            bool continueShopping = true;

            while (continueShopping)
            {
                yield return FindShelvingUnit();
                yield return PickProduct();

                // 50% chance to continue shopping
                continueShopping = Random.value < 0.5f;
            }

            if (shelvingUnit != null && shelvingUnit.IsOpen)
            {
                // Close the shelving unit if it's open (e.g., Fridges, Freezers)
                shelvingUnit.Close(true, false);
            }

            if (inventory.Count > 0)
            {
                bool isChat = Random.value < 0.5f;

                if (isChat)
                {
                    string chat = gameLanguage == "English" ? satisfiedDialogueEnglish.GetRandomLine() : satisfiedDialogueTurkish.GetRandomLine();
                    UpdateChatBubble(chat);
                }

                print("Müşteri kasaya vardı");
                StartCoroutine(WaitingTimeAtCheckout());
                yield return UpdateQueue();
                yield return StoreManager.Instance.Checkout(this);
                
            }
            else
            {
                // Customer leaves without buying anything
                if(gameLanguage == "English") { UpdateChatBubble(notFoundDialogue.GetRandomLine()); }
                else { UpdateChatBubble(notFoundDialogueTurkish.GetRandomLine());}
                
                ReputationManager.instance.RegisterCustomerFeedback(false);
                yield return StoreManager.Instance.CustomerLeave(this);
            }
        }

        private IEnumerator WaitingTimeAtCheckout()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                waitingTimeAtCheckout++;

                if(waitingTimeAtCheckout > maxWaitingTimeAtCheckout)
                {
                    waitingTimeExceeding = true;
                    yield break;
                }

            }
        }

        private IEnumerator FindShelvingUnit()
        {
            // Get a new shelving unit from the store manager.
            var newShelvingUnit = StoreManager.Instance.GetShelvingUnit();

            // If there's a current shelving unit that's different from the new one and is open, close it.
            if (shelvingUnit != null && shelvingUnit != newShelvingUnit && shelvingUnit.IsOpen)
            {
                shelvingUnit.Close(true, false);
            }

            // Assign the new shelving unit.
            shelvingUnit = newShelvingUnit;

            // If no shelving unit is available, exit the coroutine.
            if (shelvingUnit == null) yield break;

            // Unregister the shelving unit from the store manager so other customers don't target it.
            StoreManager.Instance.UnregisterShelvingUnit(shelvingUnit);

            // Set the agent's destination to the front of the shelving unit.
            agent.SetDestination(shelvingUnit.Front);

            // Wait until the agent has arrived at the shelving unit.
            while (!HasArrived())
            {
                // If the shelving unit is moving, stop the agent and exit the coroutine.
                if (shelvingUnit.IsMoving)
                {
                    agent.SetDestination(transform.position);
                    shelvingUnit = null;
                    yield break;
                }

                yield return null;
            }

            yield return LookAt(shelvingUnit.transform);
        }

        private IEnumerator PickProduct()
        {
            // If no shelving unit is available, exit the coroutine.
            if (shelvingUnit == null) yield break;

            // Get a shelf from the shelving unit.
            var shelf = shelvingUnit.GetShelf();

            // If no shelf is available or the shelving unit is moving, re-register the shelving unit and exit.
            if (shelf == null || shelvingUnit.IsMoving)
            {
                StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);
                yield break;
            }

            var product = shelf.Product;

            if (IsWillingToBuy(product))
            {
            
                inventory.Add(product);
         
                var productObj = shelf.TakeProductModel();
       
                if (!shelf.ShelvingUnit.IsOpen) shelf.ShelvingUnit.Open(true, false);
        
                float height = shelf.transform.position.y;
                string pickTrigger = "PickMedium";
                if (height < 0.5f) pickTrigger = "PickLow";
                else if (height > 1.5f) pickTrigger = "PickHigh";
           
                animator.SetTrigger(pickTrigger);
          
                yield return new WaitUntil(() => isPicking);

                Transform grip = handAttachments.Grip;

                productObj.transform.SetParent(grip);

                isPicking = false;

                productObj.transform.DOLocalRotate(Vector3.zero, 0.25f);
                productObj.transform.DOLocalMove(Vector3.zero, 0.25f);

                bool isIdle = false;
                while (!isIdle)
                {
                    isIdle = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
                    yield return null;
                }

                Destroy(productObj);

                ReputationManager.instance.RegisterCustomerFeedback(true);
     
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                string chat;
                if(gameLanguage == "English") { chat = overpricedDialogue.GetRandomLine(); }
                else { chat = overPricedDialogueTurkish.GetRandomLine(); }
                chat = chat.Replace("{product}", product.Name);
                UpdateChatBubble(chat);
                ReputationManager.instance.RegisterCustomerFeedback(false);
            }

            StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);
        }

        private bool IsWillingToBuy(Product product)
        {        
            float priceToleranceFactor = 1f + Mathf.Pow(Random.value, 2f);

            decimal maxAcceptablePrice = product.MarketPrice * (decimal)priceToleranceFactor;

            decimal customPrice = DataManager.Instance.GetCustomProductPrice(product);

            return customPrice <= maxAcceptablePrice;
        }

        private IEnumerator UpdateQueue()
        {
            // While the customer's queue number is greater than 0 (meaning they are still in the queue).
            while (queueNumber > 0)
            {
                // Get the current queue information from the store manager.
                var newQueue = StoreManager.Instance.GetQueueNumber(this);

                // Check if the customer's queue number has improved (become lower).
                if (newQueue.queueNumber < queueNumber)
                {
                    // Update the customer's queue number.
                    queueNumber = newQueue.queueNumber;

                    // Move the customer to their new queue position.
                    yield return MoveTo(newQueue.queuePosition);

                    // Make the customer look in the correct direction at their new position.
                    yield return LookAt(newQueue.lookDirection);
                }
                else
                {
                    // If the queue number hasn't improved, wait briefly before checking again.
                    yield return new WaitForSeconds(0.1f);
                }
            }
            // When the queueNumber is 0, this coroutine will stop.
        }

        public IEnumerator HandsPayment(bool isUsingCash, Cashier cashier)
        {
            bool isPaying = true;

            animator.SetBool("IsPaying", isPaying);

            handAttachments.ActivatePaymentObject(isUsingCash);

            Camera mainCamera = Camera.main;

            // Continue the payment process until isPaying is false.
            while (isPaying)
            {
                // If a cashier is available, simulate payment with the cashier (auto-scan).
                if (cashier != null)
                {
                    yield return new WaitForSeconds(0.3f);
                    cashier.TakePayment();
                    yield return new WaitForSeconds(0.7f);
                    isPaying = false;
                    print("Ödeme alındı customer");
                  
                  /*  int receivePayment = PlayerPrefs.GetInt("ReceivePayment", 0);
                    receivePayment++;
                    if(receivePayment % 4 == 0 && PlayerPrefs.GetInt("RemoveAd") != 1)
                    {
                        AdManager.instance.ShowInterstitialAd();
                    }
                    PlayerPrefs.SetInt("ReceivePayment",receivePayment); */
                    
                }
                // Otherwise, allow the player to manually process the payment (e.g., started by clicking on a payment object).
                else if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                    // Check if the raycast hits a payment object within the specified layer and range.
                    if (Physics.Raycast(ray, 10f, GameConfig.Instance.PaymentLayer))
                    {
                        isPaying = false;                   
                    }
                }

                yield return null;
            }

            animator.SetBool("IsPaying", isPaying);

            handAttachments.DeactivatePaymentObjects();
        }

        public IEnumerator MoveTo(Vector3 position)
        {
            agent.SetDestination(position);

            yield return new WaitUntil(() => HasArrived());

            // Wait for the end of the frame.
            // This can be useful for ensuring animations or other visual updates have taken place.
            yield return new WaitForEndOfFrame();
        }

        public void AskToLeave()
        {
            // If the customer has items in their inventory, they shouldn't leave yet.
            if (inventory.Count > 0) return;

            // Stop all coroutines related to the customer's current activity.
            StopAllCoroutines();

            // If the customer was interacting with a shelving unit, re-register it with the store manager.
            if (shelvingUnit != null) StoreManager.Instance.RegisterShelvingUnit(shelvingUnit);

            // Start the "CustomerLeave" coroutine to handle the customer leaving the store.
            StartCoroutine(StoreManager.Instance.CustomerLeave(this));
        }

        private IEnumerator LookAt(Transform target)
        {
            var lookDirection = (target.position - transform.position).Flatten();
            var lookRotation = Quaternion.LookRotation(lookDirection);
            yield return transform.DORotateQuaternion(lookRotation, 0.5f).WaitForCompletion();
        }

        private IEnumerator LookAt(Vector3 lookDirection)
        {
            var lookRotation = Quaternion.LookRotation(lookDirection.Flatten());
            yield return transform.DORotateQuaternion(lookRotation, 0.5f).WaitForCompletion();
        }

        private bool HasArrived()
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        animator.SetBool("IsMoving", false);
                        return true;
                    }
                }
            }

            animator.SetBool("IsMoving", true);
            return false;
        }

        public void UpdateChatBubble(string chat)
        {
            if (chatBubble != null) return;
            chatBubble = UIManager.Instance.ShowChatBubble(chat, transform);
        }

        public void OnPick(AnimationEvent animationEvent)
        {
            isPicking = true;
        }
    }
}
