using Entities.Models;
using System;
using System.Collections.Generic;

namespace Contracts.Repository
{
    public interface IAccountRepository : IRepositoryBase<Account>
    {
        PagedList<ShapedEntity> GetAccountsByOwner(Guid ownerId, AccountParameters parameters);
        ShapedEntity GetAccountByOwner(Guid ownerId, Guid id, string fields);
        Account GetAccountByOwner(Guid ownerId, Guid id);
    }
}
