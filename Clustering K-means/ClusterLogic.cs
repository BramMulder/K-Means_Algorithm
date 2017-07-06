using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Clustering_K_means
{
    class ClusterLogic
    {
        private static List<Vector> _vectors = new List<Vector>();
        private static List<Vector> _centroids = new List<Vector>();
        private static List<Vector> _prevCentroids = new List<Vector>();
        public static int AmountOfClusters;
        public static int MaxAmountOfIterations;

        private static readonly char[] Delimiters = { ';', ',' };

        public static double RunAlgorithm()
        {
            //Clear previous values
            _centroids.Clear();
            _vectors.Clear();

            //Stop program if the file cannot be found
            if (!ReadCsv()) return -1;


            PickCentroids();

            ComputeDistanceToCentroids();

            for (int iteration = 0; iteration < MaxAmountOfIterations; iteration++)
            {
                _prevCentroids = _centroids;
                RecomputeCentroids();
                ComputeDistanceToCentroids();
                //If centroid didn't change since last iteration
                if (CheckIfCentroidsChanged())
                {
                    break;
                }
            }

            return CalculateSumSquareErrors();
        }


        private static bool ReadCsv()
        {
            //_vectors = new List<Vector>();
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
                //Console.WriteLine("done reading and creating vectors");
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
            Random random = new Random();
            for (int amountOfCentroids = 0; amountOfCentroids < AmountOfClusters; amountOfCentroids++)
            {
                var r = random.Next(_vectors.Count);
                var vector = _vectors[r];

                //Select a new centroid, if the picked one is already in the _centroid list
                while (_centroids.Contains(vector))
                {
                    r = random.Next(_vectors.Count);
                    vector = _vectors[r];
                }
                _centroids.Add(vector);
            }
        }

        private static void ComputeDistanceToCentroids()
        {
            foreach (Vector vector in _vectors)
            {
                FindClosestCentroid(vector);

                //No valid centroid found
                if (vector.ClusterId == -1)
                {
                    Console.WriteLine("No valid centroid found for vector: " + vector);
                    break;
                }
            }
        }

        //Use the Pythagoras function to return the nearest centroid to the given vector
        private static void FindClosestCentroid(Vector vector)
        {
            double shortestDistance = -1;
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
                    vector.ClusterId = centroidId;
                }
            }
        }

        private static void RecomputeCentroids()
        {
            List<Vector> newCentroids = new List<Vector>();

            //For every cluster/centroid
            for (int clusterIndex = 0; clusterIndex < AmountOfClusters; clusterIndex++)
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

        private static bool CheckIfCentroidsChanged()
        {
            var xCoordinates = _centroids.Where(x => _prevCentroids.Any(y => y.Coordinates[0] == x.Coordinates[0]));
            var yCoordinates = _centroids.Where(x => _prevCentroids.Any(y => y.Coordinates[1] == x.Coordinates[1]));
            return (xCoordinates.Count() == AmountOfClusters) && (yCoordinates.Count() == AmountOfClusters);
        }
    }
}
