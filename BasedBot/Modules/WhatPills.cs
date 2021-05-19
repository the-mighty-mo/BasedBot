using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot.Modules
{
    public class WhatPills : ModuleBase<SocketCommandContext>
    {
        [Command("whatpills")]
        [Alias("what-pills")]
        public async Task WhatPillsAsync()
        {
            if (Context.User is SocketUser user)
            {
                await WhatPillsAsync(user);
            }
        }

        [Command("whatpills")]
        [Alias("what-pills")]
        public async Task WhatPillsAsync(SocketUser user)
        {
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

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}