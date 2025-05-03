using FliesProject.Models.Entities;

namespace FliesProject.ViewModel
{
    public class QuizViewModel
    {
        public Quiz Quiz { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
    }

    public class QuizResultViewModel
    {
        public int Score { get; set; }
        public UserCourseProgress CourseProgress { get; set; }
        public List<QuizWritingSubmission> WritingSubmissions { get; set; } = new List<QuizWritingSubmission>();

        // Thêm thuộc tính mới để lưu kết quả của từng câu hỏi
        public Dictionary<int, bool> QuestionResults { get; set; } = new Dictionary<int, bool>();

        // Thêm thuộc tính để lưu câu trả lời của người dùng
        public Dictionary<int, string[]> UserAnswers { get; set; } = new Dictionary<int, string[]>();
    }
}
