using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasedBot.Databases.BasedDatabaseTables
{
    public class BasedCountsTable : ITable
    {
        private readonly SqliteConnection connection;

        public BasedCountsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS BasedCounts (user_id TEXT PRIMARY KEY, based INTEGER NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetBasedCountAsync(SocketUser u)
        {
            int count = 0;

            string getUserCount = "SELECT based FROM BasedCounts WHERE user_id = @user_id;";

            using SqliteCommand cmd = new(getUserCount, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _ = int.TryParse(reader["based"].ToString(), out count);
            }
            reader.Close();

            return count;
        }

        public async Task<List<(SocketUser user, int count)>> GetAllBasedCountsAsync(SocketGuild g)
        {
            List<(SocketUser user, int count)> BasedCounts = new();

            string getBasedCounts = "SELECT user_id, based FROM BasedCounts;";

            using SqliteCommand cmd = new(getBasedCounts, connection);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _ = ulong.TryParse(reader["user_id"].ToString(), out ulong userId);
                _ = int.TryParse(reader["based"].ToString(), out int count);

                SocketUser user = g.GetUser(userId);
                if (user != null)
                {
                    BasedCounts.Add((user, count));
                }
            }
            reader.Close();

            BasedCounts.Sort(Comparer<(SocketUser user, int count)>.Create((x, y) => y.count.CompareTo(x.count)));
            return BasedCounts;
        }

        public async Task IncrementBasedCountAsync(SocketUser u)
        {
            string update = "UPDATE BasedCounts SET based = based + 1 WHERE user_id = @user_id;";
            string insert = "INSERT INTO BasedCounts (user_id, based) SELECT @user_id, 1 WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task IncrementCringeCountAsync(SocketUser u)
        {
            string update = "UPDATE BasedCounts SET based = based - 1 WHERE user_id = @user_id;";
            string insert = "INSERT INTO BasedCounts (user_id, based) SELECT @user_id, -1 WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ResetUserCountAsync(SocketUser u)
        {
            string delete = "DELETE FROM BasedCounts WHERE user_id = @user_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
