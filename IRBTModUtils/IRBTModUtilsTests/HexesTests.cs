using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using us.frostraptor.modUtils.math;

namespace HexUtilsCountingTests {


    [TestClass]
    public class HexUtilsTexts {

        [TestMethod]
        public void MaxHexes() {
            // starts at 2, goes to 3 at 3 hexes, ends at 4 at 6 hexes
            Assert.AreEqual(6, HexUtils.HexesInRange(2, 4, 3));

            // starts at 0, goes to 2 at 6 hexes, ends at 4 at 12 hexes
            Assert.AreEqual(12, HexUtils.HexesInRange(0, 4, 3));

            // starts at -4, goes to -2 at 8 hexes, ends at -1 at 12 hexes
            Assert.AreEqual(12, HexUtils.HexesInRange(-4, -1, 4));

            // starts at -6, goes to -3 at 12 hexes, ends at 0 at 24 hexes
            Assert.AreEqual(24, HexUtils.HexesInRange(-6, 0, 4));

            // starts at 2, goes to -0 at 8 hexes, ends at -2 at 16 hexes
            Assert.AreEqual(16, HexUtils.HexesInRange(2, -2, 4));

            // starts at -2, goes to 0 at 10 hexes, ends at 2 at 20 hexes
            Assert.AreEqual(20, HexUtils.HexesInRange(-2, 2, 5));

        }
    }

    [TestClass]
    public class DivisionVsIterationTest {

        [TestMethod]
        public void HexCounter() {
            Assert.AreEqual(0, HexUtils.CountHexes(0f, true));
            Assert.AreEqual(0, HexUtils.CountHexes(0f, false));

            Assert.AreEqual(1, HexUtils.CountHexes(1f, true));
            Assert.AreEqual(0, HexUtils.CountHexes(1f, false));

            Assert.AreEqual(1, HexUtils.CountHexes(29f, true));
            Assert.AreEqual(0, HexUtils.CountHexes(29f, false));

            Assert.AreEqual(1, HexUtils.CountHexes(30f, true));
            Assert.AreEqual(1, HexUtils.CountHexes(30f, false));

            Assert.AreEqual(2, HexUtils.CountHexes(31f, true));
            Assert.AreEqual(1, HexUtils.CountHexes(31f, false));
        }

        [TestMethod]
        public void DivVsIter() {
            List<float> ranges = RangesInMeters();
            List<int> divHexes = new List<int>();
            // Division
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (float range in ranges) {
                divHexes.Add(RangeToHexByDiv(range));
            }

            stopWatch.Stop();
            long divTime = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"Division time was:{divTime}ms");

            // Iteration
            List<int> iterHexes = new List<int>();
            stopWatch.Reset();
            stopWatch.Start();
            foreach (float range in ranges) {
                iterHexes.Add(HexUtils.CountHexes(range));
            }

            stopWatch.Stop();
            long iterTime = stopWatch.ElapsedMilliseconds;
            Console.WriteLine($"Iteration time was:{iterTime}ms");

            float[] rangeArr = ranges.ToArray();
            int[] divArray = divHexes.ToArray();
            int[] iterArray = iterHexes.ToArray();
            Console.WriteLine($"Testing array lengths");
            Assert.AreEqual(divArray.Length, iterArray.Length);

            Console.WriteLine($"Testing array values");
            for (int i = 0; i < divArray.Length; i++) {
                Console.WriteLine($"range:{rangeArr[i]} div:{divArray[i]} iter:{iterArray[i]}");
                Assert.AreEqual(divArray[i], iterArray[i]);
            }

            //Console.WriteLine($"Testing execution time");
            //Assert.AreEqual(divTime, iterTime);
        }

        private int RangeToHexByDiv(float range) {
            int hexes = 0;
            if (range % 30.0 == 0) {
                hexes = (int)Math.Floor(range / 30.0);
            } else {
                hexes = (int)Math.Floor(range / 30);
                hexes = hexes + 1;
            }
            return hexes;
        }

        private List<float> RangesInMeters() {
            List<float> ranges = new List<float>();
            Random rand = new Random();
            for (int i = 0; i < 1000; i++) {
                float range = rand.Next(1, 500);
                ranges.Add(range);
            }

            return ranges;
        }
    }
}
