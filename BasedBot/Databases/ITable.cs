using System.Threading.Tasks;

namespace BasedBot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
