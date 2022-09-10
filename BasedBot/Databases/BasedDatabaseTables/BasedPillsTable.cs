using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasedBot.Databases.BasedDatabaseTables
{
    public class BasedPillsTable : ITable
    {
        private readonly SqliteConnection connection;

        public BasedPillsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS BasedPills (user_id TEXT NOT NULL, pill TEXT NOT NULL, count INTEGER NOT NULL, UNIQUE(user_id, pill));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<(string pill, int count)>> GetBasedPillsAsync(SocketUser u)
        {
            List<(string pill, int count)> BasedPills = new();

            string getUserCount = "SELECT pill, count FROM BasedPills WHERE user_id = @user_id;";

            using SqliteCommand cmd = new(getUserCount, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string? pill = reader["pill"].ToString();
                if (pill == null)
                {
                    continue;
                }
                _ = int.TryParse(reader["count"].ToString(), out int count);
                BasedPills.Add((pill, count));
            }
            reader.Close();

            BasedPills.Sort(Comparer<(string pill, int count)>.Create((x, y) => y.count.CompareTo(x.count)));
            return BasedPills;
        }

        public async Task AddBasedPillAsync(SocketUser u, string pill)
        {
            string update = "UPDATE BasedPills SET count = count + 1 WHERE user_id = @user_id AND pill = @pill;";
            string insert = "INSERT INTO BasedPills (user_id, pill, count) SELECT @user_id, @pill, 1 WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
            cmd.Parameters.AddWithValue("@pill", pill);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
