using Contracts;
using Entities;
using Entities.Helpers;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Repository.Contracts
{
    public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
    {
        private ISortHelper<Owner> _sortHelper;
        private IDataShaper<Owner> _dataShaper;

        public OwnerRepository(RepositoryContext repositoryContext, 
            ISortHelper<Owner> sortHelper, IDataShaper<Owner> dataShaper)
            : base(repositoryContext)
        {
            _sortHelper = sortHelper;
            _dataShaper = dataShaper;
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

        public ExpandoObject GetOwnerById(Guid ownerId, string fields)
        {
            var owner = FindByCondition(owner => owner.Id.Equals(ownerId))
                .DefaultIfEmpty(new Owner())
                .FirstOrDefault();
            return _dataShaper.ShapeData(owner, fields);
        }

        public Owner GetOwnerById(Guid ownerId)
        {
            return FindByCondition(owner => owner.Id.Equals(ownerId))
                    .FirstOrDefault();
        }

        public PagedList<ExpandoObject> GetOwners(OwnerParameters ownerParameters)
        {
            // Filtering
            var owners = FindByCondition(o => o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
                                 o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth);
            // Searching
            SearchByName(ref owners, ownerParameters.Name);
            // Sorting
            _sortHelper.ApplySort(owners, ownerParameters.OrderBy);
            // Shaping
            var shapedOwners = _dataShaper.ShapeData(owners, ownerParameters.Fields);

            return PagedList<ExpandoObject>.ToPagedList(shapedOwners.AsQueryable(),
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

        private void SearchByName(ref IQueryable<Owner> owners, string ownerName)
        {
            if (!owners.Any() || string.IsNullOrWhiteSpace(ownerName))
                return;
            owners = owners.Where(o => o.Name.ToLower().Contains(ownerName.Trim().ToLower()));
        }

    }
}
