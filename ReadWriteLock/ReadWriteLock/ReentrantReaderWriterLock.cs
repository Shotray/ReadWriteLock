using System;
using System.Threading;

namespace ReadWriteLock
{
    public class ReentrantReaderWriterLock
    {
        private int exclusiveThreadId;
        
        private int readerCount;
        private int writerCount;

        private static object readerCountLock = new object();
        private static object writerCountLock = new object();

        private static object mutex = new object();

        private AutoResetEvent writeEvent;
        private AutoResetEvent readEvent;

        private int state;
        private static object stateLock = new object();

        private const int SHARED_SHIFT = 16;
        private const int SHARED_UNIT = (1 << SHARED_SHIFT);
        private const int EXCLUSIVE_MASK = (1 << SHARED_SHIFT) - 1;

        static int readersSharedCount(int c)
        {
            return c >> SHARED_SHIFT;
        }

        static int writersSharedCount(int c)
        {
            return c & EXCLUSIVE_MASK;
        }
        
        public ReentrantReaderWriterLock()
        {
            writeEvent = new AutoResetEvent(true);
            readEvent = new AutoResetEvent(true);
            mutex = -1;
            readerCount = 0;
            writerCount = 0;
            exclusiveThreadId = -1;
        }

        public void EnterReadLock()
        {
            int id = Environment.CurrentManagedThreadId;
            Monitor.Enter(stateLock);
            if (id == exclusiveThreadId)
            {
                state += SHARED_UNIT;
                Monitor.Exit(stateLock);
                return;
            }
            Monitor.Exit(stateLock);
            
            Monitor.Enter(mutex);
            readEvent.WaitOne();
            Monitor.Enter(readerCountLock);
            readerCount++;
            if (readerCount == 1)
            {
                writeEvent.WaitOne();
            }
            Monitor.Exit(readerCountLock);
            readEvent.Set();
            Monitor.Exit(mutex);
        }

        public void ExitReadLock()
        {
            int id = Environment.CurrentManagedThreadId;
            Monitor.Enter(stateLock);
            if (id == exclusiveThreadId)
            {
                state -= SHARED_UNIT;
                Monitor.Exit(stateLock);
                return;
            }
            Monitor.Exit(stateLock);
            
            Monitor.Enter(readerCountLock);
            readerCount--;
            if (readerCount == 0)
            {
                writeEvent.Set();
            }
            Monitor.Exit(readerCountLock);
        }

        public void EnterWriteLock()
        {
            int id = Environment.CurrentManagedThreadId;
            Monitor.Enter(stateLock);
            int r = readersSharedCount(state);
            int w = writersSharedCount(state);
            if (w != 0 && id == exclusiveThreadId)
            {
                if (r != 0)
                {
                    throw new Exception("Write lock cannot reentrant read lock");
                }
                state += 1;
                Monitor.Exit(stateLock);
                return;
            }
            Monitor.Exit(stateLock);
            
            Monitor.Enter(writerCountLock);
            writerCount++;
            if (writerCount == 1)
            {
                readEvent.WaitOne();
            }
            Monitor.Exit(writerCountLock);
            writeEvent.WaitOne();
            
            Monitor.Enter(stateLock);
            exclusiveThreadId = Environment.CurrentManagedThreadId;
            state += 1;
            Monitor.Exit(stateLock);
        }

        public void ExitWriteLock()
        {
            int id = Environment.CurrentManagedThreadId;
            Monitor.Enter(stateLock);
            state -= 1;
            if (state != 0)
            {
                Monitor.Exit(stateLock);
                return;
            }
            exclusiveThreadId = -1;
            Monitor.Exit(stateLock);

            writeEvent.Set();
            Monitor.Enter(writerCountLock);
            writerCount--;
            if (writerCount == 0)
            {
                readEvent.Set();
            }
            Monitor.Exit(writerCountLock);
        }
    }
}