using System.Threading.Tasks;

namespace API.Core
{
    public interface IUnitOfWork
    {
         Task CompleteAsync();
    }
}