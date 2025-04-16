using FliesProject.AIBot;

namespace FliesProject.Services
{
    public interface IIntentClassificationService
    {
        Task<string> ClassifyAsync(string question);
    }
    public class IntentClassificationService : IIntentClassificationService
    {
        private readonly Generator _generator;
        private readonly IAIService _aiService;
        private readonly ILogger<IntentClassificationService> _logger;

        public IntentClassificationService(IAIService aiService, ILogger<IntentClassificationService> logger,Generator generator)
        {
            _aiService = aiService;
            _logger = logger;
            _generator = generator;
        }

        public async Task<string> ClassifyAsync(string question)
        {
            if (question == null)
            {
                Console.WriteLine("Question ở trong ClassiFyAsync bị trống question");
            }
            else
            {
                Console.WriteLine("Question ở trong ClassiFyAsync Không bị trống bạn ơi trống question "+question);
            }
            Console.WriteLine("This Task is for check the type question OK!");
            try
            {
                var prompt = $@"You are an intent classification expert. You are currently an AIBot for an online English teaching website. I need you to analyze the question to return 1 topic in the 3 topics below because not only require the database to store data of this English teaching website, I also want to ask a little bit about the outside, please identify the exact topic and
Please analyze the following question and return a single word.That website will have a list of Users,Courses,Sections(this is the chapter in Courses),Lessons(this lesson is in video format),Quizzes (exercises),quiz_transactions (this is the extra-course exercise for any student, it is paid for each time doing the exercise),course_transactions(this is the table to keep information about payment to the course),enrollements(This is to store information about students' course participation),user_course_progress(This is to record the learning process of each student in each course):
- 'DATABASE' if the question is related to the database or SQL query,
- 'CODE' if the question is about coding support,
- 'GENERAL' for all other cases.
 
Question: {question}

Answer:";

                var request = new ApiRequestBuilder()
                    .WithPrompt(prompt)
                    .WithDefaultGenerationConfig(temperature: 0.7f, maxOutputTokens: 16)
                    .Build();

                var response = await _generator.GenerateContentAsync(request);
                // Giả sử AI trả về 1 từ; bạn có thể cần xử lý thêm để loại bỏ khoảng trắng, dấu chấm câu, v.v.
                return response.Result.Trim().ToUpper();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in intent classification");
                // Mặc định nếu lỗi thì gán intent là GENERAL
                return "GENERAL";
            }
        }
    }

}
