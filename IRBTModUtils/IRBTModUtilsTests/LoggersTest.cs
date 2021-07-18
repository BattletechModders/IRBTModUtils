using IRBTModUtils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace IRBTModUtilsTests
{
    [TestClass]
    public class LoggersTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestDeferringLogger()
        {
            var logger = new DeferringLogger(TestContext.TestResultsDirectory,
                "deferring_logger_test", "LVT", true, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 50000; i++)
            {
                logger.Info?.Write("Test: " + i);
            }
            sw.Stop();
            Console.WriteLine($"Execution in: {sw.ElapsedMilliseconds}ms");

            Assert.IsTrue(sw.ElapsedMilliseconds < 250);
        }

    }
}
