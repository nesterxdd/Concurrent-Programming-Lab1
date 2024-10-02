using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrent_Programming_Lab1
{
    class DataEntry
    {
        public string ID { get; set; }
        public int MatrixSize { get; set; } // Used for matrix size
        public double Seed { get; set; }    // Seed for matrix generation
    }
}
