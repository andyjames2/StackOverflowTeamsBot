using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StackOverflowBot.Repositories
{
    public class InMemoryRepository<T> : IRepository<T>
    {

        private IList<T> _collection = new List<T>();

        public void SaveOrUpdate(T item)
        {
            this._collection.Remove(item);
            this._collection.Add(item);
        }

        public void Delete(T item)
        {
            this._collection.Remove(item);
        }

        public IEnumerable<T> Get()
        {
            return new ReadOnlyCollection<T>(_collection.ToList());
        }

    }
}
