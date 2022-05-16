using System.Threading;
using System.Threading.Tasks;

namespace ReadWriteLock
{
    class TestCase3
    {
        public void Test()
        {
            System.Console.WriteLine("\nTest case 3 start!");
            ReentrantReaderWriterLock rwLock = new ReentrantReaderWriterLock();
            ManualResetEvent mre = new ManualResetEvent(false);
            int threadCount = 2;
            Task.Run(() =>
            {
                rwLock.EnterWriteLock();
                rwLock.EnterReadLock();
                rwLock.EnterReadLock();
                System.Console.WriteLine("Read lock successfully reentrant write lock!");
                rwLock.ExitReadLock();
                rwLock.ExitReadLock();
                rwLock.ExitWriteLock();
                Interlocked.Decrement(ref threadCount);
                if (threadCount == 0)
                {
                    mre.Set();
                }
            });
            Task.Run(() =>
            {
                rwLock.EnterWriteLock();
                rwLock.EnterWriteLock();
                rwLock.EnterWriteLock();
                System.Console.WriteLine("Write lock successfully reentrant write lock!");
                rwLock.ExitWriteLock();
                rwLock.ExitWriteLock();
                rwLock.ExitWriteLock();
                Interlocked.Decrement(ref threadCount);
                if (threadCount == 0)
                {
                    mre.Set();
                }
            });

            mre.WaitOne();
            Task.Run(() =>
            {
                rwLock.EnterReadLock();
                rwLock.EnterReadLock();
                System.Console.WriteLine("Read lock successfully reentrant read lock!");
                rwLock.EnterReadLock();
                rwLock.EnterReadLock();
            });
        }
    }
}