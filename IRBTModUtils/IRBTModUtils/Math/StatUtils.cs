using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace IRBTModUtils.Math
{
    public static class StatUtils
    {
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
                variance += (sample - mean) * (sample - lastMean);
                variance = variance / (iterations - 1);
            }
        }

    }
}
