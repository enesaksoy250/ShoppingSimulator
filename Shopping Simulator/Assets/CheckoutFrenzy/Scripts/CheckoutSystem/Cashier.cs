using UnityEngine;

namespace CryingSnow.CheckoutFrenzy
{
    [RequireComponent(typeof(Animator))]
    public class Cashier : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Triggers the "Take" animation on the Cashier,
        ///visually representing the action of receiving payment.
        /// </summary>
        public void TakePayment()
        {
            animator.SetTrigger("Take");
        }
    }
}
