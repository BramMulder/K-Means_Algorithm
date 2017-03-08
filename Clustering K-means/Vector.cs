namespace Clustering_K_means
{
    public class Vector
    {
        public Vector(double [] coordinates)
        {
            Coordinates = coordinates;
        }

        public double[] Coordinates;
        public double ShortestDistanceToCentroid;
        public int ClusterId;
    }
}