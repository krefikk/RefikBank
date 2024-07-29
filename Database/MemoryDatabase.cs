using RefikBank.Models;

namespace RefikBank.Database
{
    public class MemoryDatabase
    {
        public static List<User> Users { get; } = new List<User>();
        public static List<Account> Accounts { get; } = new List<Account>();
    }
}
