using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace StackOverflowBot.Repositories
{
    public class PersistedInMemoryRepository<T> : IRepository<T>
    {

        private IList<T> _collection;
        private readonly Timer _timer;
        private readonly string _filePath;

        public PersistedInMemoryRepository(IConfiguration configuration)
        {
            var path = configuration.GetValue<string>("PersistenceLocation", Path.Combine(Environment.CurrentDirectory, "persisted"));
            Directory.CreateDirectory(path);
            this._filePath = Path.Combine(path, typeof(T).Name + ".json");
            if (File.Exists(this._filePath))
            {
                using (var reader = File.OpenText(this._filePath))
                using (var jr = new JsonTextReader(reader))
                {
                    this._collection = new JsonSerializer().Deserialize<List<T>>(jr);
                }
            } else
            {
                this._collection = new List<T>();
            }
            var interval = configuration.GetValue<long>("PersistenceIntervalInMs", 0);
            if (interval == 0) interval = 30000;
            this._timer = new Timer(interval) { AutoReset = false };
            this._timer.Elapsed += (s, e) => this.Persist();
        }

        public void SaveOrUpdate(T item)
        {
            this._collection.Remove(item);
            this._collection.Add(item);
            this._timer.Enabled = true;
        }

        public void Delete(T item)
        {
            this._collection.Remove(item);
            this._timer.Enabled = true;
        }

        public IEnumerable<T> Get()
        {
            return new ReadOnlyCollection<T>(_collection.ToList());
        }

        private void Persist()
        {
            using (var writer = File.CreateText(this._filePath))
            using (var jw = new JsonTextWriter(writer))
            {
                new JsonSerializer().Serialize(jw, this._collection);
            }
        }

    }
}
