using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class HowBased : ModuleBase<SocketCommandContext>
    {
        [Command("howbased")]
        [Alias("how-based")]
        public async Task HowBasedAsync()
        {
            if (Context.User is SocketGuildUser user)
            {
                await HowBasedAsync(user);
            }
        }

        [Command("howbased")]
        [Alias("how-based")]
        public async Task HowBasedAsync(SocketGuildUser user)
        {
            Task<int> based = basedDatabase.BasedCounts.GetBasedCountAsync(user);

            List<(SocketGuildUser user, int based)> basedCounts = await basedDatabase.BasedCounts.GetAllBasedCountsAsync(Context.Guild);
            int rank = 1 + basedCounts.IndexOf((user, await based));
            string rankString = rank switch
            {
                1 => ":first_place:",
                2 => ":second_place:",
                3 => ":third_place:",
                _ => rank.ToString()
            };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{user.Mention} has a based rating of {await based}.\n" +
                    $"Rank: {rankString}");

            if (await based != 0)
            {
                embed.Description += $"\nThis user is {(await based > 0 ? "based" : "cringe")}.";
            }

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}