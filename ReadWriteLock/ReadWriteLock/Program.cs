using System;
using System.Threading;

namespace ReadWriteLock
{
    class Program
    {
        static void Main(string[] args)
        {
            new TestCase1().Test();
            new TestCase2().Test();
            new TestCase3().Test();
        }
    }
}