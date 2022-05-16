using System;
using System.Diagnostics;
using System.Threading;

namespace ReadWriteLock
{
    class TestCase2
    {
        long readWaitTime;
        long writeWaitTime;

        int writerThreadNum;
        int readerThreadNum;
        int totalThreadNum;

        private ReentrantReaderWriterLock readerWriterLock;

        int finishedWorkerCount;
        private AutoResetEvent finished;

        private object monitorLockObj;
        private int baselineWriterWaitCount;

        public TestCase2()
        {
            readWaitTime = 0;
            writeWaitTime = 0;
            readerWriterLock = new ReentrantReaderWriterLock();

            writerThreadNum = 0;
            readerThreadNum = 0;
            totalThreadNum = 1024;
            finishedWorkerCount = 0;
            finished = new AutoResetEvent(false);

            monitorLockObj = new object();
            baselineWriterWaitCount = 0;
        }

        private static void Reader(Object obj)
        {
            TestCase2 testCase = (TestCase2)obj;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            testCase.readerWriterLock.EnterReadLock();
            stopwatch.Stop();
            Interlocked.Add(ref testCase.readWaitTime, stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            Thread.Sleep(10);
            testCase.readerWriterLock.ExitReadLock();
            Interlocked.Add(ref testCase.finishedWorkerCount, 1);
            if (testCase.finishedWorkerCount == testCase.totalThreadNum)
            {
                testCase.finished.Set();
            }
        }

        private static void Writer(Object obj)
        {
            TestCase2 testCase = (TestCase2)obj;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Interlocked.Add(ref testCase.baselineWriterWaitCount, 1);
            testCase.readerWriterLock.EnterWriteLock();
            Interlocked.Add(ref testCase.baselineWriterWaitCount, -1);
            stopwatch.Stop();
            Interlocked.Add(ref testCase.writeWaitTime, stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            Thread.Sleep(100);
            testCase.readerWriterLock.ExitWriteLock();
            Interlocked.Add(ref testCase.finishedWorkerCount, 1);
            if (testCase.finishedWorkerCount == testCase.totalThreadNum)
            {
                testCase.finished.Set();
            }
        }

        private static void ReaderBaseline(Object obj)
        {
            TestCase2 testCase = (TestCase2)obj;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (testCase.baselineWriterWaitCount != 0) ;
            Monitor.Enter(testCase.monitorLockObj);
            stopwatch.Stop();
            Interlocked.Add(ref testCase.readWaitTime, stopwatch.ElapsedMilliseconds);
            Thread.Sleep(10);
            Monitor.Exit(testCase.monitorLockObj);
            Interlocked.Add(ref testCase.finishedWorkerCount, 1);
            if (testCase.finishedWorkerCount == testCase.totalThreadNum)
            {
                testCase.finished.Set();
            }
        }

        private static void WriterBaseline(Object obj)
        {
            TestCase2 testCase = (TestCase2)obj;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Monitor.Enter(testCase.monitorLockObj);
            stopwatch.Stop();
            Interlocked.Add(ref testCase.writeWaitTime, stopwatch.ElapsedMilliseconds);
            Thread.Sleep(100);
            Monitor.Exit(testCase.monitorLockObj);
            Interlocked.Add(ref testCase.finishedWorkerCount, 1);
            if (testCase.finishedWorkerCount == testCase.totalThreadNum)
            {
                testCase.finished.Set();
            }
        }

        private void printTestResult(Stopwatch stopwatch, String lockName)
        {
            Console.WriteLine(lockName + " consumes {0}ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine(lockName + " readers waiting time: {0}ms，" + lockName + " writers waiting time: {1}ms", readWaitTime, writeWaitTime);
            Console.WriteLine(lockName + " readers average waiting time: {0}ms，" + lockName + " writers average waiting time: {1}ms", readWaitTime / readerThreadNum, writeWaitTime / writerThreadNum);
        }

        public void Test()
        {
            System.Console.WriteLine("\nTest Case 2 start!");
            
            Stopwatch stopwatch = new Stopwatch();
            var rand = new Random();
            int[] randNumList = new int[totalThreadNum];
            for (int i = 0; i < totalThreadNum; i++)
                randNumList[i] = rand.Next(20);

            stopwatch.Start();
            for (int i = 0; i < totalThreadNum; i++)
            {
                int rd = randNumList[i];
                if (rd == 0)
                {
                    writerThreadNum++;
                    new Thread(Writer).Start(this);
                }
                else
                {
                    readerThreadNum++;
                    new Thread(Reader).Start(this);
                }
            }
            finished.WaitOne();
            stopwatch.Stop();
            printTestResult(stopwatch, "Ours");
            
            readerThreadNum = 0;
            writerThreadNum = 0;
            readWaitTime = 0;
            writeWaitTime = 0;
            finishedWorkerCount = 0;
            stopwatch = new Stopwatch();
            finished.Reset();
            
            stopwatch.Start();
            for (int i = 0; i < totalThreadNum; i++)
            {
                int rd = randNumList[i];
                if (rd == 0)
                {
                    writerThreadNum++;
                    new Thread(WriterBaseline).Start(this);
                }
                else
                {
                    readerThreadNum++;
                    new Thread(ReaderBaseline).Start(this);
                }
            }
            finished.WaitOne();
            stopwatch.Stop();
            printTestResult(stopwatch, "Baseline");
        }
    }
}