using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent_Programming_Lab1
{
    class ResultMonitor
    {
        private (string ID, double MatrixSum, double Seed, int MatrixSize)[] results;
        public int count { get; private set; }
        private readonly object lockObject = new object();
        private int capacity;

        public ResultMonitor(int capacity)
        {
            this.capacity = capacity;        
            results = new (string ID, double MatrixSum, double Seed, int MatrixSize)[capacity];
            count = 0;
        }

        public void Insert((string ID, double MatrixSum, double Seed, int MatrixSize) result)
        {
            lock (lockObject)
            {
                if (count < capacity)
                {
                    results[count] = result;
                    count++;
                    Array.Sort(results, 0, count, Comparer<(string ID, double MatrixSum, double Seed, int MatrixSize)>.Create((x, y) => x.MatrixSum.CompareTo(y.MatrixSum)));
                    Monitor.PulseAll(lockObject); // Notify if needed
                }
            }
        }

        public (string ID, double MatrixSum, double Seed, int MatrixSize)[] GetResults(int resultCount)
        {
            lock (lockObject)
            {
                var output = new (string ID, double MatrixSum, double Seed, int MatrixSize)[resultCount];
                Array.Copy(results, output, resultCount);
                return output;
            }
        }
    }
}
