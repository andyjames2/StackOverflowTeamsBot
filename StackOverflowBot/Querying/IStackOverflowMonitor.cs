using System.Threading.Tasks;

namespace StackOverflowBot.Querying
{
    public interface IStackOverflowMonitor
    {
        Task Query();
        void Start();
    }
}