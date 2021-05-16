using BasedBot.Databases;
using System.Threading.Tasks;

namespace BasedBot
{
    public static class DatabaseManager
    {
        public static readonly BasedDatabase basedDatabase = new();

        public static async Task InitAsync() =>
            await Task.WhenAll(
                basedDatabase.InitAsync()
            );

        public static async Task CloseAsync() =>
            await Task.WhenAll(
                basedDatabase.CloseAsync()
            );
    }
}