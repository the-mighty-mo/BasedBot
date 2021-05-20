using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class HowBased : ModuleBase<SocketCommandContext>
    {
        internal static readonly string[] basedLevels =
        {
            "not ",
            "not very ",
            "somewhat ",
            "",
            "very ",
            "incredibly ",
            "unbelievably ",
            "a god of ",
            "God of ",
        };

        [Command("howbased")]
        [Alias("how-based")]
        public async Task HowBasedAsync()
        {
            if (Context.User is SocketUser user)
            {
                await HowBasedAsync(user);
            }
        }

        [Command("howbased")]
        [Alias("how-based")]
        public async Task HowBasedAsync(SocketUser user)
        {
            Task<int> based = basedDatabase.BasedCounts.GetBasedCountAsync(user);

            List<(SocketUser user, int based)> basedCounts = await basedDatabase.BasedCounts.GetAllBasedCountsAsync(Context.Guild);
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
                    $"Rank: {rankString}\n");

            long basedLevel = await based != 0
                ? Math.Min(
                    (long)Math.Log(await based),
                    basedLevels.Length - 1
                )
                : 0;
            embed.Description += $"This user is {basedLevels[basedLevel]}based.";

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}