namespace FliesProject.Models.Entities.ViewModels
{
    public class StudentTransactionViewModel
    {
        public User User { get; set; }
        public List<TransactionHistoryViewModel> Transactions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string LatestCourseTitle { get; set; }
    }
}
