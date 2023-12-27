using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data.SearchClasses
{
    public class EntityFilterService<T> where T : class
    {
        private readonly IQueryable<T> _query;

        public EntityFilterService(IQueryable<T> query)
        {
            _query = query;
        }

        public IQueryable<T> ApplyFilter(Expression<Func<T, bool>> filter)
        {
            return _query.Where(filter);
        }
    }

}
