using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.Json;


namespace Concurrent_Programming_Lab1
{
    class Program
    {
        static DataMonitor dataMonitor; //data monitor
        static ResultMonitor resultMonitor; //result monitor
        static double filterThreshold = 10.0; //variable to filter results

        static void Main(string[] args)
        {
            string filePath = "IFU-2_NesterenkoY_L1_dat_1.json"; 
            List<DataEntry> dataEntries = LoadData(filePath);
            int n = dataEntries.Count;

           
            int monitorCapacity = n / 2;

            //initialize monitors
            dataMonitor = new DataMonitor(monitorCapacity); //initializing data monitor with monitorCapacity 
            resultMonitor = new ResultMonitor(n); //initializing result monitor with n capacity which is size of data entries
           
            int threadCount = Math.Max(2, n / 4); //number of worker threads (2 ≤ x ≤ n/4)

            

            //start worker threads
            Thread[] workerThreads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                workerThreads[i] = new Thread(WorkerThread);
                workerThreads[i].Start();
            }

            // Main thread fills data monitor
            foreach (var entry in dataEntries)
            {
                dataMonitor.Insert(entry);
            }
            dataMonitor.MarkAsComplete();

            // Wait for all worker threads to complete
            foreach (var thread in workerThreads)
            {
                thread.Join();
            }

            // Write results to a text file
            WriteResultsToFile("IFU-2_NesterenkoY_L1_rez.txt");
            Console.WriteLine("Results are written to file IFU-2_NesterenkoY_L1_rez.txt");
        }

        /// <summary>
        /// Worker thread function
        /// </summary>
        static void WorkerThread()
        {
            while (true)
            {
                DataEntry entry = dataMonitor.Remove();
                if (entry == null) break; //exit if there is no data left

                //perform algorithm logic
                double[,] matrix = GenerateMatrix(entry.MatrixSize, entry.Seed);                
                double[,] transposedMatrix = TransposeMatrix(matrix);
                double[,] resultMatrix = MultiplyMatrixByItself(transposedMatrix);
                double sum = ComputeMatrixSum(resultMatrix);

                //apply filter
                if (sum > filterThreshold)
                {
                    resultMonitor.Insert((entry.ID, sum));
                }
            }
        }


        /// <summary>
        /// Function to load data from a JSON file into a dynamic list
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static List<DataEntry> LoadData(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<DataEntry>>(json);
        }

        /// <summary>
        /// Function to generate a matrix based on size and seed
        /// </summary>
        /// <param name="size"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        static double[,] GenerateMatrix(int size, double seed)
        {
            double[,] matrix = new double[size, size];
            Random random = new Random((int)Math.Floor(seed));

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = random.NextDouble();
                }
            }
            return matrix;
        }

        /// <summary>
        /// Function to multiply a matrix by itself
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        static double[,] MultiplyMatrixByItself(double[,] matrix)
        {
            int size = matrix.GetLength(0);
            double[,] result = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < size; k++)
                    {
                        result[i, j] += matrix[i, k] * matrix[k, j];
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Function to compute the sum of matrix elements
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        static double ComputeMatrixSum(double[,] matrix)
        {
            double sum = 0;
            int size = matrix.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    sum += matrix[i, j];
                }
            }
            return sum;
        }

        /// <summary>
        /// Function to transpose a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        static double[,] TransposeMatrix(double[,] matrix)
        {
            int size = matrix.GetLength(0);
            double[,] transposed = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }
            return transposed;
        }

        /// <summary>
        /// Function to write results to a text file
        /// </summary>
        /// <param name="filePath"></param>
        static void WriteResultsToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("ID\tMatrixSum");
                for (int i = 0; i < resultMonitor.count; i++)
                {
                    var results = resultMonitor.GetResults(resultMonitor.count);
                    //writer.WriteLine($"{results[i].ID}\t{results[i].MatrixSum:F2}");
                    writer.WriteLine("{0}\t{1:f2}", results[i].ID, results[i].MatrixSum);
                }
            }
        }
    }
}
