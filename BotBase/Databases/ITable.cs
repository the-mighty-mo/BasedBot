using System.Threading.Tasks;

namespace BotBase.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
