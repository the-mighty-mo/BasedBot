using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class BasedLeaderboard : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("based-leaderboard", "Gets a based leaderboard of the top 5 users")]
        public async Task BasedLeaderboardAsync()
        {
            List<(SocketUser user, int based)> basedCounts = await basedDatabase.BasedCounts.GetAllBasedCountsAsync(Context.Guild);
            IEnumerable<(SocketUser user, int based)> topFive = basedCounts.Take(5);

            string leaderboard = "";
            int rank = 1;
            foreach ((SocketUser user, int based) in topFive)
            {
                string rankString = rank switch
                {
                    1 => ":first_place:",
                    2 => ":second_place:",
                    3 => ":third_place:",
                    _ => $"\u200b {rank} \u200b \u200b"
                };
                long basedLevel = based != 0
                    ? Math.Min(
                        (long)Math.Log(based),
                        HowBased.basedLevels.Length - 1
                    )
                    : 0;
                leaderboard += $"{rankString} - {user.Mention}: {based} ({HowBased.basedLevels[basedLevel]}based)\n";
                rank++;
            }

            if (leaderboard == "")
            {
                leaderboard = "no users are based";
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithCurrentTimestamp();

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Based Leaderboard")
                .WithValue(leaderboard);
            embed.AddField(field);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}