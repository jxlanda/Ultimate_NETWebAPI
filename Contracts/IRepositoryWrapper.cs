using Contracts.IRepositoryBase;
using Contracts.Repository;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IOwnerRepository Owner { get; }
        IAccountRepository Account { get; }
        IUserRepository User { get; }
        void Save();
        Task SaveAsync();
    }
}
