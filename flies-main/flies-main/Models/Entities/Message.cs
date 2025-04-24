namespace FliesProject.Models.Entities;

public class Message
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int MentorId { get; set; } // Thêm cột MentorId
    public string Sender { get; set; }
    public string Content { get; set; }
    public string Time { get; set; }
    public User? Student { get; set; }
    public User? Mentor { get; set; } // Liên kết với Mentor
}