using Contracts;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Contracts
{
    public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
    {
        public OwnerRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public void CreateOwner(Owner owner)
        {
            Create(owner);
        }

        public void DeleteOwner(Owner owner)
        {
            Delete(owner);
        }

        public IEnumerable<Owner> GetAllOwners()
        {
            return FindAll()
                .OrderBy(ow => ow.Name)
                .ToList();
        }

        public Owner GetOwnerById(Guid ownerId)
        {
            return FindByCondition(owner => owner.Id.Equals(ownerId))
                    .FirstOrDefault();
        }

        public PagedList<Owner> GetOwners(OwnerParameters ownerParameters)
        {
            var owners = FindByCondition(o => o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
                                o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth)
                            .OrderBy(on => on.Name);

            return PagedList<Owner>.ToPagedList(owners,
                    ownerParameters.PageNumber,
                    ownerParameters.PageSize);
        }

        public Owner GetOwnerWithDetails(Guid ownerId)
        {
            return FindByCondition(owner => owner.Id.Equals(ownerId))
                .Include(ac => ac.Accounts)
                .FirstOrDefault();
        }

        public void UpdateOwner(Owner owner)
        {
            Update(owner);
        }
    }
}
