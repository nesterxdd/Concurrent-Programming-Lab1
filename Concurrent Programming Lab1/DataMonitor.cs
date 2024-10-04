using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent_Programming_Lab1
{
    class DataMonitor
    { 

        private DataEntry[] data;
        private int count;
        private readonly object lockObject = new object();
        private int capacity;
        public bool isComplete { get; private set; } = false;

        public DataMonitor(int capacity)
        {
            count = 0;
            this.capacity = capacity;
            data = new DataEntry[capacity];
        }

        public void Insert(DataEntry entry)
        {
            lock (lockObject)
            {
                while (count >= capacity)
                {
                    Monitor.Wait(lockObject); // Wait if full
                }
                data[count] = entry;
                count++;
                Monitor.PulseAll(lockObject); // Notify workers
            }
        }

        public DataEntry Remove()
        {
            lock (lockObject)
            {
                while (count == 0)
                {
                    if (isComplete)
                    {
                        return null; // Return null if no more data will be added
                    }
                    Monitor.Wait(lockObject); // Wait if empty
                }
                DataEntry entry = data[--count];
                Monitor.PulseAll(lockObject); // Notify other threads
                return entry;
            }
        }

        public int GetCount()
        {
            return count;
        }

        public void MarkAsComplete()
        {
            lock (lockObject)
            {
                isComplete = true; // Signal that no more data will be added
                Monitor.PulseAll(lockObject); // Notify all waiting threads
            }
        }

    }
}
