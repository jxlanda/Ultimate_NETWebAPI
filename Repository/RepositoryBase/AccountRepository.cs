using Contracts.Repository;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Contracts
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {

		public AccountRepository(RepositoryContext repositoryContext)
			: base(repositoryContext)
		{
		}

		public PagedList<ShapedEntity> GetAccountsByOwner(Guid ownerId, AccountParameters parameters)
		{
			var accounts = Get(a => a.OwnerId.Equals(ownerId));

			var sortedAccounts = ApplySort(accounts.AsQueryable(), parameters.OrderBy);

			var shapedAccounts = ShapeData(sortedAccounts, parameters.Fields);

			return PagedList<ShapedEntity>.ToPagedList(shapedAccounts,
				parameters.PageNumber,
				parameters.PageSize);
		}

		public ShapedEntity GetAccountByOwner(Guid ownerId, Guid id, string fields)
		{
			var account = Get(a => a.OwnerId.Equals(ownerId) && a.Id.Equals(id)).SingleOrDefault();
			return ShapeDataSingle(account, fields);
		}

		public Account GetAccountByOwner(Guid ownerId, Guid id)
		{
			return Get(a => a.OwnerId.Equals(ownerId) && a.Id.Equals(id)).SingleOrDefault();
		}
	}
}
