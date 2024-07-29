namespace RefikBank.Models
{
    public class TransferRequest
    {
        public string SourceAccountNumber { get; set; }
        public int SourceAccountType { get; set; }
        public string TargetAccountNumber { get; set; }
        public int TargetAccountType { get; set; }
        public float Amount { get; set; }
    }
}
