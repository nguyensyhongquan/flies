using System.ComponentModel.DataAnnotations;

namespace FliesProject.Models.AImodel
{
    public class ChatViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập câu hỏi")]
        [Display(Name = "Câu hỏi")]
        public string Question { get; set; }

        [Display(Name = "Kết quả")]
        public string? Answer { get; set; }

        [Display(Name = "Biểu đồ")]
        public string? ChartData { get; set; }

        [Display(Name = "Loại biểu đồ")]
        public string? ChartType { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }

        [Display(Name = "Loại cơ sở dữ liệu")]
        public string? DatabaseType { get; set; }

        [Display(Name = "Chuỗi kết nối")]
        public string? ConnectionString { get; set; }
    }
}
