using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace StackOverflowBot.Commands
{
    public class HelpCommand : ICommand
    {

        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        public HelpCommand(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public async Task<bool> Do(IEnumerable<string> args)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = "@ me or message me directly to tell me what to do. Here's what I can do:\n\n\r\n\u00A0\n\n\r\n" +
                "**subscribe** - Subscribe to all new questions and answers on linked Stack Overflow teams.\n\n\r\n" +
                "**subscribe tag1 tag2 ...** - Subscribe to questions and answers that are tagged with the given tags.\n\n\r\n" +
                "**unsubscribe** - Unsubscribe from all questions and answers.\n\n\r\n" +
                "**link** - Link a new Stack Overflow team to get new questions and answers from.\n\n\r\n" +
                "**unlink** - Stop receiving new questions and answers from a Stack Overflow team.\n\n\r\n" +
                "**undo** - Undo the last thing I was asked to do.\n";
            activity.TextFormat = TextFormatTypes.Markdown;
            await this._turnContext.SendActivityAsync(activity, cancellationToken: this._cancellationToken);
            return false;
        }

        public async Task Undo()
        {
        }

    }
}
