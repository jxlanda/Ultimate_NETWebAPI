using Contracts.Repository;
using Entities;
using Entities.Models;

namespace Repository.Contracts
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
    }
}
