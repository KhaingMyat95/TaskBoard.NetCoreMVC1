namespace TaskManager.CoreMVC.Models
{
    public class ResponseModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Url { get; set; }
    }
}
