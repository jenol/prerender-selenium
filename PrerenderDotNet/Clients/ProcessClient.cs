using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace PrerenderDotNet.Clients
{
    public class ProcessClient : IProcessClient
    {
        private readonly string fileName;
        public ProcessClient(string fileName)
        {
            this.fileName = fileName;
        }

        public void AddId(int id)
        {
            WithWriter(writer => writer.WriteLine(id), false);
        }

        public void DeleteOrphanedProcesses()
        {
            var ids = GetKnownIds();

            foreach (var id in ids)
            {
                Process.GetProcessById(id).Kill();
            }

            Clear();
        }

        public void Clear()
        {
            WithWriter(writer => { }, true);
        }

        public int[] GetKnownIds()
        {
            var ids = new List<int>();
            return WithReader(reader =>
            {
                if (reader == null)
                {
                    return ids.ToArray();
                }

                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (int.TryParse(line, out var id))
                    {
                        ids.Add(id);
                    }
                }

                return ids.ToArray();
            });
        }

        private T WithReader<T>(Func<StreamReader, T> func)
        {
            using IsolatedStorageFileStream isoStream = new(fileName, FileMode.OpenOrCreate, Store);
            using StreamReader reader = new(isoStream);
            return func(reader);
        }

        private void WithWriter(Action<StreamWriter> action, bool truncate)
        {
            using var isoStream = Store.FileExists(fileName)
                ? new IsolatedStorageFileStream(fileName, truncate ? FileMode.Truncate : FileMode.Append, Store)
                : new IsolatedStorageFileStream(fileName, FileMode.CreateNew, Store);
            using StreamWriter writer = new(isoStream);
            action(writer);
        }

        private static IsolatedStorageFile Store => IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
    }
}
