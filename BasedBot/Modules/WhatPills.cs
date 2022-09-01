using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class WhatPills : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("what-pills", "Gets what pills a user has")]
        public async Task WhatPillsAsync(SocketUser user = null)
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

            List<(string pill, int count)> basedPills = await basedDatabase.BasedPills.GetBasedPillsAsync(user);

            string pills = "";
            foreach ((string pill, int count) in basedPills)
            {
                pills += $"{pill} (x{count})\n";
            }

            pills = pills == "" ? $"{user.Mention} has no pills" : $"**{user.Mention} Pills:**\n{pills}";
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription(pills);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}