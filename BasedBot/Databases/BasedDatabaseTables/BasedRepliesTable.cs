using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasedBot.Databases.BasedDatabaseTables
{
    public class BasedRepliesTable : ITable
    {
        private readonly SqliteConnection connection;

        public BasedRepliesTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS BasedReplies (user_id TEXT NOT NULL, message_id TEXT NOT NULL, UNIQUE(user_id, message_id));", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> HasRepliedAsync(SocketUser u, SocketUserMessage m)
        {
            bool hasReplied;

            string getBasedReply = "SELECT user_id FROM BasedReplies WHERE user_id = @user_id AND message_id = @message_id;";

            using SqliteCommand cmd = new(getBasedReply, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
            cmd.Parameters.AddWithValue("@message_id", m.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            hasReplied = await reader.ReadAsync().ConfigureAwait(false);

            reader.Close();

            return hasReplied;
        }

        public async Task AddRepliedAsync(SocketUser u, SocketUserMessage m)
        {
            string insert = "INSERT INTO BasedReplies (user_id, message_id) SELECT @user_id, @message_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM BasedReplies WHERE user_id = @user_id AND message_id = @message_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
            cmd.Parameters.AddWithValue("@message_id", m.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
