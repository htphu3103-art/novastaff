using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces;

public interface IPresenceTracker
{
    Task UserConnected(int userID, string connectionId);
    Task UserDisconnected(int userID, string connectionId);
    bool IsOnline(int userID);
    int[] GetOnlineUserIDs();
}
