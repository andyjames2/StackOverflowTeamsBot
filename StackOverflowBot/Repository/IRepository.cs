using System.Collections.Generic;

namespace StackOverflowBot.Repositories
{
    public interface IRepository<T>
    {

        void SaveOrUpdate(T item);

        void Delete(T item);

        IEnumerable<T> Get();

    }
}