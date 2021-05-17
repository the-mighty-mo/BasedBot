using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static BasedBot.DatabaseManager;

namespace BasedBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        private static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            CommandServiceConfig config = new()
            {
                DefaultRunMode = RunMode.Async
            };
            commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage() =>
            await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private async Task SendDisconnectError(Exception e) =>
            await Console.Out.WriteLineAsync(e.Message);

        private async Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) => await Task.Run(() => msg.Author.Id == client.CurrentUser.Id);

        private async Task<bool> ShouldDeleteBotCommands() => await Task.Run(() => true);

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands())
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds);
            }

            await ManageBasedAsync(msg);
        }

        private async Task ManageBasedAsync(SocketUserMessage msg)
        {
            // make sure this is a reply to someone else's message
            if (msg.ReferencedMessage is SocketUserMessage repliedMsg && repliedMsg.Author is SocketUser user && user != msg.Author)
            {
                // make sure the message is based and isn't a duplicate reply
                if (repliedMsg.Content.ToLower().StartsWith("based") && !await basedDatabase.BasedReplies.HasRepliedAsync(msg.Author, repliedMsg))
                {
                    // farming protection
                    if (repliedMsg.ReferencedMessage is SocketUserMessage superMsg && superMsg.Author == msg.Author && superMsg.Content.ToLower().StartsWith("based"))
                    {
                        return;
                    }
                    // increment the target user's based rating
                    await Task.WhenAll(
                        basedDatabase.BasedCounts.IncrementBasedCountAsync(user),
                        basedDatabase.BasedReplies.AddRepliedAsync(msg.Author, repliedMsg)
                    );
                }
            }
        }
    }
}