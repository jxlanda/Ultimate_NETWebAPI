using Contracts;
using Entities;
using Entities.Extensions;
using Entities.Helpers;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Contracts
{
	public class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
	{
		//private ISortHelper<Owner> _sortHelper;
		//private IDataShaper<Owner> _dataShaper;

		public OwnerRepository(RepositoryContext repositoryContext
			//ISortHelper<Owner> sortHelper,
			//IDataShaper<Owner> dataShaper
			)
			: base(repositoryContext)
		{
			//_sortHelper = sortHelper;
			//_dataShaper = dataShaper;
		}

		public PagedList<ShapedEntity> GetOwners(OwnerParameters ownerParameters)
		{
			var owners = Get(o => o.DateOfBirth.Year >= ownerParameters.MinYearOfBirth &&
										o.DateOfBirth.Year <= ownerParameters.MaxYearOfBirth);

			SearchByName(owners.AsQueryable(), ownerParameters.Name);

			var sortedOwners = ApplySort(owners.AsQueryable(), ownerParameters.OrderBy);
			var shapedOwners = ShapeData(sortedOwners, ownerParameters.Fields);

			return PagedList<ShapedEntity>.ToPagedList(shapedOwners,
				ownerParameters.PageNumber,
				ownerParameters.PageSize);
		}

		private void SearchByName(IQueryable<Owner> owners, string ownerName)
		{
			if (!owners.Any() || string.IsNullOrWhiteSpace(ownerName))
				return;

			if (string.IsNullOrEmpty(ownerName))
				return;

			owners = owners.Where(o => o.Name.ToLowerInvariant().Contains(ownerName.Trim().ToLowerInvariant()));
		}

		public ShapedEntity GetOwnerById(Guid ownerId, string fields)
		{
			var owner = Get(owner => owner.Id.Equals(ownerId))
				.DefaultIfEmpty(new Owner())
				.FirstOrDefault();

			return ShapeDataSingle(owner, fields);
		}

		public Owner GetOwnerById(Guid ownerId)
		{
			return Get(owner => owner.Id.Equals(ownerId))
				.DefaultIfEmpty(new Owner())
				.FirstOrDefault();
		}

		public void CreateOwner(Owner owner)
		{
			Insert(owner);
		}

		public void UpdateOwner(Owner dbOwner, Owner owner)
		{
			dbOwner.Map(owner);
			Update(dbOwner);
		}

		public void DeleteOwner(Owner owner)
		{
			Delete(owner);
		}
	}
}
