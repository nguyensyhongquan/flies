namespace FliesProject.ViewModel
{
    public class QuizSubmitViewModel
    {
        public int QuizId { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public Dictionary<int, string[]> Answers { get; set; }
        public Dictionary<int, string> WritingAnswers { get; set; }
    }
}
