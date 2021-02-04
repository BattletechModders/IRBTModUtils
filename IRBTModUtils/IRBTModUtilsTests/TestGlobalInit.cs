using IRBTModUtils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtilsTests
{
    [TestClass]
    public static class TestGlobalInit
    {
        [AssemblyInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            IRBTModUtils.Mod.Log = new DeferringLogger(testContext.TestResultsDirectory,
                "lowvis_tests", "LVT", true, true);

            IRBTModUtils.Mod.Config = new IRBTModUtils.ModConfig();
        }

        [AssemblyCleanup]
        public static void TearDown()
        {

        }
    }
}
