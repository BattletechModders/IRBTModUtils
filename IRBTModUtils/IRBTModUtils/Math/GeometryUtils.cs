using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace us.frostraptor.modUtils.math {

    public static class GeometryUtils {       

        // Shamelessly stolen from http://csharphelper.com/blog/2014/07/find-the-centroid-of-a-polygon-in-c/
        public static Vector3 FindCentroid(List<Vector3> actorPositions) {

            float centroidX = 0f;
            float centroidY = 0f;
            Vector3[] points = MakeClosedPolygon(actorPositions);
            int xSignSum = 0;
            int ySignSum = 0;
            for (int i = 0; i < actorPositions.Count; i++) {
                float x_0 = points[i].x;
                float x_1 = points[i + 1].x;
                float y_0 = points[i].y;
                float y_1 = points[i + 1].y;

                xSignSum += Math.Sign(x_0);
                ySignSum += Math.Sign(y_0);

                float secondFactor = (x_0 * y_1) - (x_1 * y_0);

                float resultX = (x_0 + x_1) * secondFactor;

                float resultY = (y_0 + y_1) * secondFactor;

                centroidX += resultX;
                centroidY += resultY;
            }

            float zPosAverage = actorPositions.Select(v => v.z).Sum();
            zPosAverage = zPosAverage / actorPositions.Count;

            // Divide by 6 times the polygon area
            float polygonArea = SignedPolygonArea(actorPositions);
            centroidX = centroidX / (6 * polygonArea);
            centroidY = centroidY / (6 * polygonArea);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (centroidX < 0) {
                centroidX = -1 * centroidX;
                centroidY = -1 * centroidY;
            }

            // Try to reconcile the signs. Because we use a +/- scale, the signs screw up the calculation.
            if (xSignSum == actorPositions.Count * -1 && centroidX > 0) {
                centroidX = centroidX * -1;
            } else if (xSignSum == actorPositions.Count && centroidX < 0) {
                centroidX = centroidX * -1;
            }

            if (ySignSum == actorPositions.Count * -1 && centroidY > 0) {
                centroidY = centroidY * -1;
            } else if (ySignSum == actorPositions.Count && centroidY < 0) {
                centroidY = centroidY * -1;
            }

            return new Vector3(centroidX, centroidY, zPosAverage);
        }

        // Shamelessly stolen from http://csharphelper.com/blog/2014/07/calculate-the-area-of-a-polygon-in-c/
        public static float SignedPolygonArea(List<Vector3> objectPositions) {
            float area = 0f;

            Vector3[] points = MakeClosedPolygon(objectPositions);
            for (int i = 0; i < objectPositions.Count; i++) {
                area +=
                    (points[i + 1].x - points[i].x) *
                    (points[i + 1].y + points[i].y) / 2;
            }

            return area;
        }

        private static Vector3[] MakeClosedPolygon(List<Vector3> objectPositions) {
            Vector3[] areaPoints = new Vector3[objectPositions.Count + 1];
            Vector3[] rawPoints = objectPositions.ToArray();
            for (int i = 0; i < objectPositions.Count; i++) {
                areaPoints[i] = new Vector3(rawPoints[i].x, rawPoints[i].y);
                if (i == 0) {
                    areaPoints[objectPositions.Count] = new Vector3(rawPoints[0].x, rawPoints[0].y);
                }
            }
            return areaPoints;
        }

    }

}
