using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackOverflowBot.Commands
{

    public interface ICommand
    {

        Task<bool> Do(IEnumerable<string> args);

        Task Undo();

    }

}
