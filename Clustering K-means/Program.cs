using System;

namespace Clustering_K_means
{
    public class Program
    {
        private static int _amountOfAlgorithmIterations = 0;

        static void Main(string[] args)
        {
            ProcessUserInput();
            double SSE = 0;
            for (int i = 1; i <= _amountOfAlgorithmIterations; i++)
            {
                var x = ClusterLogic.RunAlgorithm();
                if (SSE == 0 || x < SSE)
                    SSE = x;
            }
            Console.WriteLine(SSE);
        }

        //Quick and dirty user input read
        public static void ProcessUserInput()
        {
            Console.WriteLine("Please Specify the amount of clusters , the amount of iterations of the algorithm, " +
                              "\n the amount times running the algoirthm - in format -- clusters, centroid iterations, algorithm iterations");
            var userInput = Console.ReadLine();

            try
            {
                var splitUserInput = userInput?.Split(',');
                ClusterLogic.AmountOfClusters = int.Parse(splitUserInput[0]);
                ClusterLogic.MaxAmountOfIterations = int.Parse(splitUserInput[1]);
                _amountOfAlgorithmIterations = int.Parse(splitUserInput[2]);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid input, try again");
                ProcessUserInput();
            }
        }
    }
}