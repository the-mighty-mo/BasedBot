using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class HowBased : InteractionModuleBase<SocketInteractionContext>
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

        [SlashCommand("how-based", "Gets how based a user is and their rank on the leaderboard")]
        public async Task HowBasedAsync(SocketUser user = null)
        {
            if (user == null)
            {
                if (Context.User is SocketUser u)
                {
                    user = u;
                }
                else
                {
                    return;
                }
            }

            Task<int> based = basedDatabase.BasedCounts.GetBasedCountAsync(user);

            EmbedBuilder embed;
            if (Context.Guild is not null)
            {
                List<(SocketUser user, int based)> basedCounts = await basedDatabase.BasedCounts.GetAllBasedCountsAsync(Context.Guild);
                int rank = 1 + basedCounts.IndexOf((user, await based));
                string rankString = rank switch
                {
                    1 => ":first_place:",
                    2 => ":second_place:",
                    3 => ":third_place:",
                    _ => rank.ToString()
                };

                embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{user.Mention} has a based rating of {await based}.\n" +
                        $"Rank: {rankString}\n");
            }
            else
            {
                embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{user.Mention} has a based rating of {await based}.\n");
            }

            long basedLevel = await based != 0
                ? Math.Min(
                    (long)Math.Log(await based),
                    basedLevels.Length - 1
                )
                : 0;
            embed.Description += $"This user is {basedLevels[basedLevel]}based.";

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}