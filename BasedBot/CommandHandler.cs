using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
            if (!result.IsSuccess && info.GetValueOrDefault()?.RunMode == RunMode.Async && result.Error is not (CommandError.UnknownCommand or CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private Task SendConnectMessage() =>
            Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private Task SendDisconnectError(Exception e) =>
            Console.Out.WriteLineAsync(e.Message);

        private Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) => Task.Run(() => msg.Author.Id == client.CurrentUser.Id);

        private Task<bool> ShouldDeleteBotCommands() => Task.Run(() => true);

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

        private static async Task ManageBasedAsync(SocketUserMessage msg)
        {
            // make sure this is a reply to someone else's message
            if (msg.ReferencedMessage is SocketUserMessage repliedMsg && repliedMsg.Author is SocketUser user && user != msg.Author)
            {
                Task<bool> hasReplied = basedDatabase.BasedReplies.HasRepliedAsync(msg.Author, repliedMsg);

                Regex regex = new(@"^\W*based(?:\s+and((?:\s+\S+\s*)+)(?<!-)(?:-)?pilled)?\W*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var match = regex.Matches(msg.Content).Cast<Match>().FirstOrDefault();

                // make sure the message is based and isn't a duplicate reply
                if (match != null && !await hasReplied)
                {
                    // farming protection
                    if (repliedMsg.ReferencedMessage is SocketUserMessage superMsg && superMsg.Author == msg.Author && regex.IsMatch(superMsg.Content))
                    {
                        return;
                    }

                    List<Task> cmds = new()
                    {
                        // increment the target user's based rating
                        basedDatabase.BasedCounts.IncrementBasedCountAsync(user),
                        basedDatabase.BasedReplies.AddRepliedAsync(msg.Author, repliedMsg)
                    };

                    string pill = match.Groups.Values.Select(x => x.Value).Skip(1).FirstOrDefault()?.Trim();
                    if (pill != null && pill.Length is > 0 and <= 35)
                    {
                        cmds.Add(basedDatabase.BasedPills.AddBasedPillAsync(user, pill));
                    }

                    await Task.WhenAll(cmds);
                }
            }
            else
            {
                Regex regex = new(@"^\W*?(?<!<)(?:<@!\d+>\s*)+\s+\W*based(?:\s+and((?:\s+\S+\s*)+)(?<!-)(?:-)?pilled)?\W*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var match = regex.Matches(msg.Content).Cast<Match>().FirstOrDefault();

                // make sure the message is based
                if (match != null)
                {
                    string pill = match.Groups.Values.Select(x => x.Value).Skip(1).FirstOrDefault()?.Trim();

                    IEnumerable<Task> IncrementBasedForUsers()
                    {
                        bool validPill = pill != null && pill.Length is > 0 and <= 35;
                        foreach (var user in msg.MentionedUsers.Distinct().Where(u => u != msg.Author))
                        {
                            yield return basedDatabase.BasedCounts.IncrementBasedCountAsync(user);

                            if (validPill)
                            {
                                yield return basedDatabase.BasedPills.AddBasedPillAsync(user, pill);
                            }
                        }
                    }

                    IEnumerable<Task> cmds = IncrementBasedForUsers();
                    await Task.WhenAll(cmds);
                }
            }
        }
    }
}