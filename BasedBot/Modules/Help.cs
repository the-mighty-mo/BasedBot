using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace BasedBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            EmbedFieldBuilder prefix = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Prefix")
                .WithValue("\\" +
                    "\n**or**\n" +
                    Context.Client.CurrentUser.Mention + "\n\u200b");
            embed.AddField(prefix);

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Commands")
                .WithValue(
                    "ping\n" +
                    "  - Returns the bot's Server and API latencies\n\n" +
                    "how-based (user mention/user ID)\n" +
                    "  - Gets how based a user is and their rank on the leaderboard\n\n" +
                    "based-leaderboard\n" +
                    "  - Gets a based leaderboard of the top 5 users\n\n" +
                    "what-pills (user mention/user ID)\n" +
                    "  - Gets what pills a user has\n\n" +
                    "Reply to a user's message with \"based\" if they said something based.\n" +
                    "Feel free to mention if what they said is also pilled."
                );
            embed.AddField(field);

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}