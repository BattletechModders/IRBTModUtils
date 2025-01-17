using System.Runtime.CompilerServices;


namespace IRBTModUtils.Math
{
    public static class StatUtils
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WelfordAlgorithm(
            double     sample, 
            ref int   iterations, 
            ref double mean, 
            ref double lastMean, 
            ref double variance)
        {
            // Avoids division by zero
            if (iterations < 1)
            {
                // Cannot compute
                variance = 0;
            }
            else
            {
                lastMean = mean;
                mean += (sample - mean) / iterations;
                // Should reconsider statistics and possibly do a double pass. Might be in underflow NaN
                //variance += (sample - mean) * (sample - lastMean);
                //variance = variance / (iterations - 1);
            }
        }

    }
}
