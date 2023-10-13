using Microsoft.AspNetCore.OData.Results;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Radical.Uniques
{
    public sealed class UniqueOne<T> : UniqueOne, IUniqueOne<T> where T : IUnique
    {
        public UniqueOne(T entity) : base(entity.ToQueryable()) { }

        public UniqueOne(IEnumerable<T> enumerable) : base(enumerable.AsQueryable()) { }

        public UniqueOne(IQueryable<T> queryable) : base(queryable) { }

        [JsonIgnore]
        public override IQueryable<T> Queryable => base.Queryable as IQueryable<T>;
    }

    public abstract class UniqueOne : SingleResult
    {
        public UniqueOne(object entity) : base(entity.ToQueryable()) { }

        public UniqueOne(IQueryable queryable) : base(queryable) { }

        [JsonIgnore]
        public virtual new IQueryable Queryable => base.Queryable;
    }
}
