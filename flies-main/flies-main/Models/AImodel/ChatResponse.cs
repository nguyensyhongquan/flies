namespace FliesProject.Models.AImodel
{
    public class ChatResponse
    {
        public string Message { get; set; }
        public string? ChartData { get; set; }
        public string? ChartType { get; set; }
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
