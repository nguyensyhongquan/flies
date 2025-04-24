using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI;

namespace FliesProject.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int studentId, int mentorId, string sender, string content, string time)
        {
            // Gửi tin nhắn đến nhóm cụ thể (dựa trên cặp StudentId-MentorId)
            string groupName = GetGroupName(studentId, mentorId);

            await Clients.Group($"Chat_{studentId}_{mentorId}").SendAsync("ReceiveMessage", studentId, mentorId, sender, content, time);
            //await Clients.Group(groupName).SendAsync("ReceiveMessage", studentId, mentorId, sender, content, time);
        }

        public async Task JoinChat(int studentId, int mentorId)
        {
            try
            {
                string groupName = GetGroupName(studentId, mentorId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinChat: {ex.Message}");
                throw; // Re-throw to ensure the client receives the error
            }
        }


        public async Task LeaveChat(int studentId, int mentorId)
        {
            // Xóa client khỏi nhóm chat
            string groupName = GetGroupName(studentId, mentorId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private string GetGroupName(int studentId, int mentorId)
        {
            // Tạo tên nhóm duy nhất cho cặp StudentId-MentorId
            return $"Chat_{studentId}_{mentorId}";
        }
    }
}
