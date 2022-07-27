using BasedBot.Databases;
using System.Threading.Tasks;

namespace BasedBot
{
    public static class DatabaseManager
    {
        public static readonly BasedDatabase basedDatabase = new();

        public static Task InitAsync() =>
            Task.WhenAll(
                basedDatabase.InitAsync()
            );

        public static Task CloseAsync() =>
            Task.WhenAll(
                basedDatabase.CloseAsync()
            );
    }
}