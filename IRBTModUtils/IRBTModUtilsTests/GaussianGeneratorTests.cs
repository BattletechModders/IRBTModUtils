using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using us.frostraptor.modUtils.Redzen;

namespace RedzenTests {
    [TestClass]
    public class GaussianGeneratorTests {
        [TestMethod]
        public void TestSampleBounds() {
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = 0;
            double stdDev = 4;
            double[] sampleBuf = new double[4096];
            ZigguratGaussian.Sample(rng, mean, stdDev, sampleBuf);

            Dictionary<int, int> frequency = new Dictionary<int, int>();
            foreach (double sample in sampleBuf) {
                int normalizedVal = 0;
                if (sample > 0) {
                    normalizedVal = (int)Math.Floor(sample);
                } else if (sample < 0) {
                    normalizedVal = (int)Math.Ceiling(sample);
                } 
                if (frequency.ContainsKey(normalizedVal)) {
                    frequency[normalizedVal] = frequency[normalizedVal] + 1;
                } else {
                    frequency[normalizedVal] = 1;
                }                                
            }

            List<int> orderedKeys = frequency.OrderByDescending(kvp => kvp.Key)
                .Select(kvp => kvp.Key)
                .ToList();            
            foreach (int key in orderedKeys) {
                Console.WriteLine($"Value:{key} had frequency:{frequency[key]}");
            }
        }

        [TestMethod]
        public void TestTimeFor4096() {
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = 0;
            double stdDev = 4;
            double[] sampleBuf = new double[8172];

            Stopwatch timer = new Stopwatch();
            for (int i = 0; i < 10; i++) {
                timer.Start();
                ZigguratGaussian.Sample(rng, mean, stdDev, sampleBuf);
                timer.Stop();
                Console.WriteLine($"Execution took: {timer.Elapsed} to generate 8172 elements.");
                timer.Reset();
            }
            
        }
    }
}
