using FliesProject.Models.AImodel;
using System.Text;
using System.Text.Json;
using System.Text;

namespace FliesProject.Services
{
    public interface IChatService
    {
        Task<ChatViewModel> SendMessage(ChatViewModel model);
        Task<ChatViewModel> AnalyzeDatabase(ChatViewModel model);
        Task<ChatViewModel> CheckConnection(ChatViewModel model);
    }

 
}
