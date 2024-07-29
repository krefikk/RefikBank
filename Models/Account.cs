namespace RefikBank.Models
{
    public class Account
    {
        public string accountNumber; // IBAN like
        private float balance;
        public List<string> history;
        public int accountType; // Like 0 for TL, 1 for dollars account, 2 for euros account, 3 for time deposit TL account and goes on...

        public float GetBalance()
        {
            return balance;
        }

        public void SetBalance(float amount) 
        {
            balance = amount;
        }

        public void DecreaseBalance(float decreaseAmount)
        {
            if (decreaseAmount <= balance)
            {
                balance -= decreaseAmount;
            }
        }

        public void IncreaseBalance(float increaseAmount)
        {
            if (increaseAmount >= 0)
            {
                balance += increaseAmount;
            }
        }

        public List<string> GetHistory() 
        {
            return history;
        }

        public void AddHistoryLog(string log) 
        {
            history.Add(log);
        }

    }
}
