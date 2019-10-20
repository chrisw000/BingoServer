using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public interface IChatHub
    {
        Task ReceiveMessage(string user, string message);
    }
}