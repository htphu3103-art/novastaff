using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces;

// Interface — không đổi dù dùng memory hay Redis
public interface IPresenceTracker
{
    Task UserConnectedAsync(int userID, string connectionId);
    Task UserDisconnectedAsync(int userID, string connectionId);
    Task<bool> IsOnlineAsync(int userID);          
    Task<int[]> GetOnlineUserIDsAsync();           
}
