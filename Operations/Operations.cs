using RefikBank.Database;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RefikBank.Operations
{
    public class Operations
    {
        public string CreateUniqueAccountNumber(int accountType)
        {
            // 10 haneli unique bir sayı döndürür.
            int length = 9;
            const string sample = "0123456789";
            StringBuilder uniqueNumber = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(sample.Length);
                uniqueNumber.Append(sample[index]);
            }
            string uniqueAccountNumber = "TR" + uniqueNumber.ToString() + accountType.ToString();

            // Buradaki algoritmayı sadece son oluşturulan account number'ı hafızada tutan ve oluşturulacak sıradaki account number'ı onu 1 arttırarak oluşturan bir algoritmaya dönüştürürsek verim inanılmaz artar.
            // Check for uniqueness
            bool isUnique = false;
            while (!isUnique)
            {
                isUnique = true;
                foreach (var account in MemoryDatabase.Accounts)
                {
                    if (uniqueAccountNumber == account.accountNumber)
                    {
                        isUnique = false;
                        uniqueNumber.Clear();
                        for (int k = 0; k < length; k++)
                        {
                            int index = random.Next(sample.Length);
                            uniqueNumber.Append(sample[index]);
                        }
                        uniqueAccountNumber = "TR" + uniqueNumber.ToString() + accountType.ToString();
                        break;
                    }
                }
            }
            return uniqueAccountNumber;
        }

        public string CreateUniqueUserID()
        {
            // 9 haneli unique bir sayı döndürür.
            int length = 6;
            const string sampleLetters = "ABCDEFGHIJKLMNOPRSTUVYZXWQ";
            const string sampleNumbers = "0123456789";
            StringBuilder uniqueID = new StringBuilder();
            Random random = new Random();

            // For first part, only letters
            for (int i = 0; i < length / 2; i++)
            {
                int index = random.Next(sampleLetters.Length);
                uniqueID.Append(sampleLetters[index]);
            }
            // For first part, only numbers
            for (int i = 0; i < length / 2; i++)
            {
                int index = random.Next(sampleNumbers.Length);
                uniqueID.Append(sampleNumbers[index]);
            }
            string uniqueUserID = uniqueID.ToString();

            // Buradaki algoritmayı sadece son oluşturulan account number'ı hafızada tutan ve oluşturulacak sıradaki account number'ı onu 1 arttırarak oluşturan bir algoritmaya dönüştürürsek verim inanılmaz artar.
            // Check for uniqueness
            bool isUnique = false;
            while (!isUnique)
            {
                isUnique = true;
                foreach (var user in MemoryDatabase.Users)
                {
                    if (uniqueUserID == user.userID)
                    {
                        isUnique = false;
                        uniqueID.Clear();
                        // For first part, only letters
                        for (int i = 0; i < length / 2; i++)
                        {
                            int index = random.Next(sampleLetters.Length);
                            uniqueID.Append(sampleLetters[index]);
                        }
                        // For first part, only numbers
                        for (int i = 0; i < length / 2; i++)
                        {
                            int index = random.Next(sampleNumbers.Length);
                            uniqueID.Append(sampleNumbers[index]);
                        }
                        uniqueUserID = uniqueID.ToString();
                        break;
                    }
                }
            }
            return uniqueUserID;
        }

        public float ConvertCurrency(float amount, int sourceType, int targetType)
        {
            // Conversion logic based on account types
            // Example conversion rates, these should be replaced with real conversion logic
            float conversionRate = GetConversionRate(sourceType, targetType);
            return amount * conversionRate;
        }

        public float GetConversionRate(int sourceType, int targetType)
        {
            // Exchange rates in 26/07/2024 at 15.45
            if (sourceType == 0 && targetType == 1) // TL to USD
                return 0.032f;
            if (sourceType == 0 && targetType == 2) // TL to EUR
                return 0.035f;
            if (sourceType == 1 && targetType == 0) // USD to TL
                return 32.95f;
            if (sourceType == 1 && targetType == 2) // USD to EUR
                return 0.92f;
            if (sourceType == 2 && targetType == 0) // EUR to TL
                return 35.77f;
            if (sourceType == 2 && targetType == 1) // EUR to USD
                return 1.09f;

            // Return 1 if source and target types are the same
            return 1.0f;
        }

        public int GetAccountTypeFromAccountNumber(string accountNumber) 
        {
            string lastElement = accountNumber.Substring(accountNumber.Length - 1);
            int accountType = Int32.Parse(lastElement);
            return accountType;
        }

    }
}
