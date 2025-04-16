namespace FliesProject.Services
{
    public class ChatRouterService
    {
        private readonly IEnumerable<IChatModule> _chatModules;
        private readonly IIntentClassificationService _intentService;
        public ChatRouterService(IEnumerable<IChatModule> chatModules, IIntentClassificationService intentService)
        {
            _chatModules = chatModules;
            _intentService = intentService;
   
        }

        /// <summary>
        /// Xác định module phù hợp dựa vào câu hỏi và chuyển tiếp xử lý.
        /// </summary>
        /// <param name="question"></param>
        /// <returns>Phản hồi từ module</returns>
        public async Task<string> RouteQuestion(string question)
        {
            Console.WriteLine("Start the Route Question if not print the errol i");
            Console.WriteLine("The question is"+question);
            if(question == null)
            {
                Console.WriteLine("Question trống rồi mi ơi");
            }
            else
            {
                Console.WriteLine("Question không trống nhưng k nhận diện ra được");
            }
            // Gọi AI để phân tích câu hỏi
            string intent = await _intentService.ClassifyAsync(question);
            Console.WriteLine("the intent is ::::"+intent);
            if (intent == "DATABASE")
            {
                var dbModule = _chatModules.FirstOrDefault(m => m is DatabaseChatModule);
                if (dbModule != null)
                    return await dbModule.ProcessQuestion(question);
            }
         
            else // intent GENERAL hoặc không xác định
            {
                Console.WriteLine("nó đang thực hiện general nè");
                var generalModule = _chatModules.FirstOrDefault(m => m is GeneralChatModule);
                if (generalModule != null)
                {
                    Console.WriteLine("generalModule k hề null nha mầy");
                    return await generalModule.ProcessQuestion(question);
                }
                else
                {
                    Console.WriteLine("generalModule  null nha mầy");
                }
                   
            }

            return "Xin lỗi, tôi không hiểu câu hỏi của bạn.";
        }
    }

}
