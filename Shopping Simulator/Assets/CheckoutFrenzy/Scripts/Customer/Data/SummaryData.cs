namespace CryingSnow.CheckoutFrenzy
{
    [System.Serializable]
    public class SummaryData
    {
        public int TotalCustomers { get; set; }
        
       
        public decimal PreviousBalance { get; set; }
        public decimal TotalRevenues { get; set; }
        public decimal TotalSpending { get; set; }

        public SummaryData(decimal currentBalanceDouble) // double constructor
        {
            PreviousBalance = currentBalanceDouble;
     
        }

        public SummaryData() // Parametresiz constructor (JsonUtility için gereklidir)
        {
            TotalCustomers = 0;
            PreviousBalance = 0.0m;
            TotalRevenues = 0.0m;
            TotalSpending = 0.0m;
        }
    }
}
