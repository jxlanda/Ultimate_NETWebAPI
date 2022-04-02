using Entities.Models;
using System.Collections.Generic;

namespace Entities.Helpers
{
    public interface IDataShaper<T>
    {
        IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString, Dictionary<string, string> childFields = null);
        ShapedEntity ShapeDataSingle(object entity, string fieldsString);
    }
}
