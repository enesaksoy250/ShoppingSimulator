namespace CryingSnow.CheckoutFrenzy
{
    public enum BillType
    {
        Rent,
        Electricity,
        Salary,
        Repayment
    }

    public enum BillStatus
    {
        Unpaid,
        Paid,
        Charged
    }

    [System.Serializable]
    public class Bill
    {
        public BillType Type { get; set; }
        public int IssueDay { get; set; }
        public int DueDay { get; set; }
        public int GracePeriodDays { get; set; }
        public decimal Amount { get; set; }
        public decimal LatePenalty { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Unpaid;

        public bool IsPaid => Status == BillStatus.Paid;

        private int currentDay => DataManager.Instance.Data.TotalDays;

        public bool IsOverdue() =>
            !IsPaid && currentDay > DueDay + GracePeriodDays;

        public bool IsInGracePeriod() =>
            !IsPaid && currentDay > DueDay && currentDay <= DueDay + GracePeriodDays;

        public decimal GetTotalAmountDue()
        {
            if (IsPaid) return 0m;
            return IsOverdue() ? Amount + LatePenalty : Amount;
        }

        public string GetStatusText()
        {
            if (IsPaid)
                return LanguageManager.instance.GetLocalizedValue("PaidText");

            if (currentDay < DueDay)
            {
                int daysLeft = DueDay - currentDay;
                if (daysLeft == 1)
                    return LanguageManager.instance.GetLocalizedValue("DueTomorrowText");
                if (daysLeft == 0)
                    return LanguageManager.instance.GetLocalizedValue("DueTodayText");
                //return $"{daysLeft} days left";
                return LanguageManager.instance.GetLocalizedValue("DaysLeftText").Replace("{days}",daysLeft.ToString());
            }
            else if (currentDay == DueDay)
            {
                return LanguageManager.instance.GetLocalizedValue("DueTodayText");
            }
            else if (currentDay > DueDay && currentDay <= DueDay + GracePeriodDays)
            {
                int graceLeft = DueDay + GracePeriodDays - currentDay;
                if (graceLeft == 1)
                    return LanguageManager.instance.GetLocalizedValue("GraceEndsTomorrowText");
                if (graceLeft == 0)
                    return LanguageManager.instance.GetLocalizedValue("GraceEndsTodayText");
                //return $"Grace period ({graceLeft} days left)";
                return LanguageManager.instance.GetLocalizedValue("GracePeriodDaysLeftText").Replace("{graceLeft}",graceLeft.ToString());
            }
            else
            {
                return LanguageManager.instance.GetLocalizedValue("OverdueText");
            }
        }
    }
}
