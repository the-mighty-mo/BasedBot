using BasedBot.Databases.BasedDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasedBot.Databases
{
    public class BasedDatabase
    {
        private readonly SqliteConnection connection = new("Filename=Based.db");
        private readonly Dictionary<System.Type, ITable> tables = new();

        public BasedCountsTable BasedCounts => tables[typeof(BasedCountsTable)] as BasedCountsTable;
        public BasedRepliesTable BasedReplies => tables[typeof(BasedRepliesTable)] as BasedRepliesTable;
        public BasedPillsTable BasedPills => tables[typeof(BasedPillsTable)] as BasedPillsTable;

        public BasedDatabase()
        {
            tables.Add(typeof(BasedCountsTable), new BasedCountsTable(connection));
            tables.Add(typeof(BasedRepliesTable), new BasedRepliesTable(connection));
            tables.Add(typeof(BasedPillsTable), new BasedPillsTable(connection));
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await Task.WhenAll(GetTableInits());
        }

        public async Task CloseAsync() => await connection.CloseAsync();
    }
}