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
        public async Task HowBasedAsync(SocketUser? user = null)
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

            Task<int> basedTask = basedDatabase.BasedCounts.GetBasedCountAsync(user);
            int based;

            EmbedBuilder embed;
            if (Context.Guild != null)
            {
                List<(SocketUser user, int based)> basedCounts = await basedDatabase.BasedCounts.GetAllBasedCountsAsync(Context.Guild).ConfigureAwait(false);
                based = await basedTask.ConfigureAwait(false);
                int rank = 1 + basedCounts.IndexOf((user, based));
                string rankString = rank switch
                {
                    1 => ":first_place:",
                    2 => ":second_place:",
                    3 => ":third_place:",
                    _ => rank.ToString()
                };

                embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{user.Mention} has a based rating of {based}.\n" +
                        $"Rank: {rankString}\n");
            }
            else
            {
                based = await basedTask.ConfigureAwait(false);
                embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{user.Mention} has a based rating of {based}.\n");
            }

            long basedLevel = based != 0
                ? Math.Min(
                    (long)Math.Log(based),
                    basedLevels.Length - 1
                )
                : 0;
            embed.Description += $"This user is {basedLevels[basedLevel]}based.";

            await Context.Interaction.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}