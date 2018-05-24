namespace ACE.Diag
{
    /// <summary>
    /// Random number generator
    /// </summary>
    public class Random
    {
        public static System.Random RandomNumberGenerator = new System.Random();

        /// <summary>
        /// Returns an integer between min and max INCLUSIVE
        /// ie., Next(0-1) can return 0 or 1
        /// </summary>
        /// <param name="min">The minimum number to return</param>
        /// <param name="max">The maximum number to return</param>
        /// <returns>A random integer between min and max, inclusive.</returns>
        public static int Next(int min, int max)
        {
            var random = RandomNumberGenerator.Next(min, max + 1);
            //Console.WriteLine(string.Format("Random({0}, {1}) generated {2}", min, max, random));
            return random;
        }
    }
}
