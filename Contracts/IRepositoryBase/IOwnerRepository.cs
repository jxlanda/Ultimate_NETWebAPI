using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Contracts
{
    public interface IOwnerRepository : IRepositoryBase<Owner>
    {
        // Not paged
        IEnumerable<Owner> GetAllOwners();
        // Paged
        PagedList<ExpandoObject> GetOwners(OwnerParameters ownerParameters);
        ExpandoObject GetOwnerById(Guid ownerId, string fields);
        Owner GetOwnerById(Guid ownerId);
        Owner GetOwnerWithDetails(Guid ownerId);
        void CreateOwner(Owner owner);
        void UpdateOwner(Owner owner);
        void DeleteOwner(Owner owner);
    }
}
