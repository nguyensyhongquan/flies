namespace FliesProject.Models.Entities.ViewModels
{
    public class TransactionHistoryViewModel
    {
        public string TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
