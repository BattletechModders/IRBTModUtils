using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using us.frostraptor.modUtils.math;

namespace GeometryTests {

    [TestClass]
    public class GeometryTests {

        [TestMethod]
        public void CentroidTest() {
            List<Vector3> testPositions = new List<Vector3>() {
                new Vector3(-840f, 133.4f, 23f),
                new Vector3(-708f, 128.9f, 473.3f),
                new Vector3(-744f, 127.9f, 228.7f),
                new Vector3(-768f, 128f, 128f)
            };
            Vector3 centroid = GeometryUtils.FindCentroid(testPositions);
            Assert.AreEqual(-769.8802f, centroid.x);
            Assert.AreEqual(129.9787f, centroid.y);
            Assert.AreEqual(213.25f, centroid.z);
        }
    }
}
