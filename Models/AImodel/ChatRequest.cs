using System.ComponentModel.DataAnnotations;

namespace FliesProject.Models.AImodel
{
    public class ChatRequest
    {
        [Required]
        public string Message { get; set; }

        public string? DatabaseType { get; set; }

        public string? ConnectionString { get; set; }

        public bool GenerateChart { get; set; }

        public string? ChartType { get; set; }

    }
}
