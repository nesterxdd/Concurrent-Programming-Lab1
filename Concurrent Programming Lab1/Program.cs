﻿using System;
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
        static double filterThreshold = 50.0; //variable to filter results
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
                if (entry == null) break; // exit if there is no data left

                // Generate matrix for the current entry
                double[,] matrix = GenerateMatrix(entry.MatrixSize, entry.Seed);

                // Find the longest path sum in the matrix
                double longestPathSum = FindLongestPathSum(matrix);
              

                // Apply filter
                if (longestPathSum > filterThreshold)
                {
                    resultMonitor.Insert((entry.ID, longestPathSum));
                }
            }
        }

        /// <summary>
        /// Function to find the longest path sum in a matrix
        /// </summary>
        static double FindLongestPathSum(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double maxPathSum = 0;

            // Create a visited array
            bool[,] visited = new bool[rows, cols];

            // Start DFS from each cell in the matrix
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    maxPathSum = Math.Max(maxPathSum, DFS(matrix, visited, i, j));
                }
            }
            return maxPathSum;
        }

        /// <summary>
        /// Depth-first search to find the longest path sum
        /// </summary>
        static double DFS(double[,] matrix, bool[,] visited, int x, int y)
        {
            // Check bounds
            if (x < 0 || y < 0 || x >= matrix.GetLength(0) || y >= matrix.GetLength(1))
                return 0;

            // Mark the cell as visited
            visited[x, y] = true;

            double maxPath = 0;

            // Explore all four possible directions
            int[] dx = { 0, 1, 0, -1 }; // Right, Down, Left, Up
            int[] dy = { 1, 0, -1, 0 };

            for (int dir = 0; dir < 4; dir++)
            {
                int newX = x + dx[dir];
                int newY = y + dy[dir];

                // Only proceed if the new cell is within bounds and not visited
                if (newX >= 0 && newY >= 0 && newX < matrix.GetLength(0) && newY < matrix.GetLength(1) && !visited[newX, newY])
                {
                    maxPath = Math.Max(maxPath, DFS(matrix, visited, newX, newY));
                }
            }

            // Unmark the cell (backtracking)
            visited[x, y] = false;

            // Store the maximum path sum from this cell
            return matrix[x, y] + maxPath;
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
                    matrix[i, j] = random.NextDouble() * 10; // Values between 0 and 10
                }
            }
            return matrix;
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
