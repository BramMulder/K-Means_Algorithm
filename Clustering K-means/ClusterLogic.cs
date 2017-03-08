using System;
using System.Collections.Generic;
using System.IO;

namespace Clustering_K_means
{
    class ClusterLogic
    {
        private static List<Vector> _vectors = new List<Vector>();
        private static List<Vector> _centroids = new List<Vector>();
        private static int _amountOfClusters;
        private static int _maxAmountOfIterations;


        private static readonly char[] Delimiters = { ';', ',' };

        static void Main(string[] args)
        {
            ProcessUserInput();
            //Stop program if the file cannot be found
            if (!ReadCsv()) return;
            PickCentroids();
            ComputeDistanceToCentroids();

            for (int iteration = 0; iteration < _maxAmountOfIterations; iteration++)
            {
                RecomputeCentroids();
                ComputeDistanceToCentroids();
            }

            Console.WriteLine("SSE: " + CalculateSumSquareErrors());
        }

        //Quick and dirty user input read
        private static void ProcessUserInput()
        {
            Console.WriteLine("Please Specify the amount of clusters , the amount of iterations - in format -- clusters, iterations");
            var userInput = Console.ReadLine();
            
            try
            {
                var splitUserInput = userInput?.Split(',');
                _amountOfClusters = int.Parse(splitUserInput[0]);
                _maxAmountOfIterations = int.Parse(splitUserInput[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid input, try again");
                ProcessUserInput();
            }

        }

        private static bool ReadCsv()
        {
            bool fileExists = File.Exists(@"WineDataFlipped.CSV");

            if (!fileExists)
            {
                Console.WriteLine("File Could not be located");
                return false;
            }

            using (StreamReader reader = new StreamReader(@"WineDataFlipped.CSV"))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    var fields = line.Split(Delimiters[0]);
                    CreateVectors(fields[0]);
                }
                Console.WriteLine("done reading and creating vectors");
            }
            return true;
        }

        private static void CreateVectors(string fields)
        {
            try
            {
                var vector = new Vector(Array.ConvertAll(fields.Split(Delimiters[1]), double.Parse));
                _vectors.Add(vector);
            }
            catch (FormatException)
            {
                Console.WriteLine("{0}: Bad Format", fields);
            }
            catch (OverflowException)
            {
                Console.WriteLine("{0}: Overflow", fields);
            }
        }

        private static void PickCentroids()
        {
            var random = new Random();
            for (int i = 0; i < _amountOfClusters; i++)
            {
                var r = random.Next(_vectors.Count);
                var vector = _vectors[r];
                _centroids.Add(vector);
            }
        }

        private static void ComputeDistanceToCentroids()
        {
            foreach (Vector vector in _vectors)
            {
                vector.ClusterId = FindClosestCentroid(vector);
                //No valid centroid found
                if (vector.ClusterId == -1)
                {
                    Console.WriteLine("No valid centroid found for vector: " + vector);
                    break;
                }
            }
        }

        //Use the Pythagoras function to return the nearest centroid to the given vector
        private static int FindClosestCentroid(Vector vector)
        {
            double shortestDistance = -1;
            int closestCentroidId = -1;
            //Find the shortest distance from the given vector to a centroid
            for (int centroidId = 0; centroidId < _centroids.Count; centroidId++)
            {
                double distance = 0;

                //For every centroid dimension - Subtract every coordinate from the centroid
                for (int dimension = 0; dimension < _centroids[centroidId].Coordinates.Length; dimension++)
                {
                    double delta = _centroids[centroidId].Coordinates[dimension] - vector.Coordinates[dimension];
                    distance = distance + Math.Pow(delta, 2);
                }
                distance = Math.Sqrt(distance);

                if (shortestDistance == -1 || distance < shortestDistance)
                {
                    shortestDistance = distance;
                    vector.ShortestDistanceToCentroid = distance;
                    closestCentroidId = centroidId;
                }
            }

            return closestCentroidId;
        }

        private static void RecomputeCentroids()
        {
            List<Vector> newCentroids = new List<Vector>();

            //For every cluster/centroid
            for (int clusterIndex = 0; clusterIndex < _amountOfClusters; clusterIndex++)
            {
                newCentroids.Add(CalculateCentroidPerCluster(clusterIndex));
            }
            _centroids = newCentroids;
        }

        private static Vector CalculateCentroidPerCluster(int clusterIndex)
        {
            Vector newCentroid = new Vector(new double[32]);
            //Total amount of vectors in a cluster
            int totalAmountVectors = 0;

            //Sum all the vectors with the same cluster ID
            foreach (Vector vector in _vectors)
            {
                if (vector.ClusterId != clusterIndex) continue;
                //Add every dimension of the selected vector to the existing vector
                for (int dimension = 0; dimension < vector.Coordinates.Length; dimension++)
                {
                    newCentroid.Coordinates[dimension] = newCentroid.Coordinates[dimension] + vector.Coordinates[dimension];
                }
                totalAmountVectors++;
            }

            for (int s = 0; s < newCentroid.Coordinates.Length; s++)
            {
                newCentroid.Coordinates[s] = newCentroid.Coordinates[s]/totalAmountVectors;
            }

            return newCentroid;
        }

        private static double CalculateSumSquareErrors()
        {
            double totalSSE = 0;

            if(_vectors.Count == 0)
                return -1;

            foreach (var vector in _vectors)
            {
                var squaredShortestDistance = Math.Pow(vector.ShortestDistanceToCentroid, 2);
                totalSSE = totalSSE + squaredShortestDistance;
            }

            return totalSSE;
        }

    }
}
