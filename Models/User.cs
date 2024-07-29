using RefikBank.Operations;
using RefikBank.Database;

namespace RefikBank.Models
{
    public class User
    {
        public string userID;
        private string mail;
        private string password;
        public List<Account> accounts;

        public string GetPassword() 
        {
            return password;
        }

        public void SetPassword(string password)
        {
            this.password = password;
        }

        public string GetMail()
        {
            return mail;
        }

        public void SetMail(string mail)
        {
            this.mail = mail;
        }

        public List<Account> GetAccounts()
        {
            return accounts;
        }

        public void CreateAccountsList(List<Account> accounts) 
        {
            accounts = new List<Account>();
        }

        public void CreateNewAccount(int accountType)
        {
            Operations.Operations operations = new Operations.Operations();
            string accountNumber = operations.CreateUniqueAccountNumber(accountType);

            Account newAccount = new Account
            {
                accountNumber = accountNumber,
                accountType = operations.GetAccountTypeFromAccountNumber(accountNumber),
                history = new List<string>()
            };

            // Set balance
            newAccount.SetBalance(0);

            // Add new history log
            DateTime now = DateTime.Now;
            newAccount.GetHistory().Add("Account created: " + now);

            accounts.Add(newAccount);
            MemoryDatabase.Accounts.Add(newAccount); // Add the new account to the memory database
        }
    }
}
