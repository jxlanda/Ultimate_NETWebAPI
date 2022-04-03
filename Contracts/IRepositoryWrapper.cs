using Contracts.IRepositoryBase;
using Contracts.Repository;

namespace Contracts
{
    public interface IRepositoryWrapper
    {
        IOwnerRepository Owner { get; }
        IAccountRepository Account { get; }
        IUserRepository User { get; }
        void Save();
    }
}
