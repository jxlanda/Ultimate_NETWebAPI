using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.Repository
{
    public interface IAccountRepository : IRepositoryBase<Account>
    {
        IEnumerable<Account> AccountsByOwner(Guid ownerId);
    }
}
