using Contracts;
using Contracts.Repository;
using Entities;
using Entities.Helpers;
using Entities.Models;
using Repository.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private RepositoryContext _repoContext;
        private IOwnerRepository _owner;
        private IAccountRepository _account;
        private ISortHelper<Owner> _ownerSortHelper;
        private ISortHelper<Account> _accountSortHelper;
        private IDataShaper<Owner> _ownerDataShaper;
        public IOwnerRepository Owner
        {
            get
            {
                if (_owner == null)
                {
                    _owner = new OwnerRepository(_repoContext, _ownerSortHelper, _ownerDataShaper);
                }
                return _owner;
            }
        }
        public IAccountRepository Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new AccountRepository(_repoContext);
                }
                return _account;
            }
        }
        public RepositoryWrapper(RepositoryContext repositoryContext, ISortHelper<Owner> ownerSortHelper,
            ISortHelper<Account> accountSortHelper,
            IDataShaper<Owner> ownerDataShaper)
        {
            _repoContext = repositoryContext;
            _ownerSortHelper = ownerSortHelper;
            _accountSortHelper = accountSortHelper;
            _ownerDataShaper = ownerDataShaper;
        }
        public void Save()
        {
            _repoContext.SaveChanges();
        }
    }
}
