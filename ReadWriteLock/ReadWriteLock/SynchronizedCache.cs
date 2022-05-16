using System.Collections.Generic;

namespace ReadWriteLock
{
    public class SynchronizedCache
    {
        private ReentrantReaderWriterLock cacheLock = new ReentrantReaderWriterLock();
        private Dictionary<int, string> innerCache = new Dictionary<int, string>();

        public int Count
        { get { return innerCache.Count; } }

        public string Read(int key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public void Add(int key, string value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(key, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public void Delete(int key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
    }
}