using System.Collections.Generic;

namespace CryingSnow.CheckoutFrenzy
{
    [System.Serializable]
    public class GameData
    {
        public string StoreName { get; set; }
        public decimal PlayerMoney { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentExperience { get; set; }

        public List<FurnitureData> SavedFurnitures { get; set; }
        public List<BoxData> SavedBoxes { get; set; }

        public List<FurnitureBoxData> SavedFurnitureBoxes { get; set; }
        public List<CustomPrice> CustomPrices { get; set; }

        public decimal PendingOrdersValue { get; set; }
        public decimal UnpaidProductsValue { get; set; }

        //public HashSet<int> LicensedProducts { get; set; }
        public List<int> LicensedProducts { get; set; }

        public List<int> OwnedLicenses { get; set; }
        public bool IsWarehouseUnlocked { get; set; }
        public int ExpansionLevel { get; set; }

        public int TotalDays { get; set; }
        public int TotalMinutes { get; set; }

        public SummaryData CurrentSummary { get; set; }
        public MissionData CurrentMission { get; set; }

        public List<EmployeeData> HiredEmployees { get; set; }

        public List<Bill> Bills { get; set; }
        public List<Loan> Loans { get; set; }

        public List<CleanableData> SavedCleanables { get; set; }

        public GameData()
        {
            SavedFurnitures = new List<FurnitureData>();
            SavedBoxes = new List<BoxData>();
            SavedFurnitureBoxes = new List<FurnitureBoxData>();
            CustomPrices = new List<CustomPrice>();
            LicensedProducts = new List<int>();
            Bills = new List<Bill>();
            Loans = new List<Loan>();
            SavedCleanables = new();
        }


        public void Initialize()
        {
            StoreName = GameConfig.Instance.DefaultStoreName;

            PlayerMoney = GameConfig.Instance.StartingMoney;
            CurrentLevel = 1;
            SavedFurnitureBoxes = new List<FurnitureBoxData>();
            SavedFurnitures = new List<FurnitureData>();
            SavedBoxes = new List<BoxData>();
            CustomPrices = new List<CustomPrice>();
            //LicensedProducts = new HashSet<int>();
            LicensedProducts = new List<int>();
            OwnedLicenses = new List<int>();

            TotalDays = 1;

            var openTime = GameConfig.Instance.OpenTime;
            int totalMinutes = TimeRange.ToMinutes(openTime.StartHour, openTime.StartMinute);
            TotalMinutes = totalMinutes;
            HiredEmployees = new List<EmployeeData>();
            Bills = new List<Bill>();
            Loans = new List<Loan>();
            CurrentSummary = new SummaryData(PlayerMoney);
            CurrentMission = new MissionData(1);
            SavedCleanables = new();

        }
   

    }
}
