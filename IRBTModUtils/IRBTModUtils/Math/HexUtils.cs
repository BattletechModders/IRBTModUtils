using System;

namespace us.frostraptor.modUtils.math {

    public static class HexUtils {

        // Count the number of hexes (rounded up) in a range
        public static int CountHexes(float range, bool roundUp=true) {
            int hexes = 0;
            int count = 0;
            while (count < range) {
                count += 30;
                hexes++;
            }

            if (hexes != 0 && !Equals(hexes * 30, (int)range) && !roundUp) { 
                hexes = hexes - 1; 
            }
            //Console.WriteLine($"For range:{range} roundUp:{roundUp} = hexes:{hexes}");

            return hexes;
        }

        public static int CountSteps(float range, int step, bool roundUp=true) {
            int hexes = CountHexes(range, false);
            int steps = 0;
            int count = 0;
            while (count <= hexes) {
                count+= step;
                steps++;
            }

            if (steps != 0 && !roundUp) { steps = steps - 1; }

            //Console.WriteLine($"For range:{range} hexes:{hexes} step:{step} = steps:{steps}");
            return steps;
        }

        public static int DecayingModifier(int start, int end, int step, float range) {
            int steps = CountSteps(range, step, false);

            if (start < end) {
                // Increasing range
                int mod = start + steps;
                return mod > end ? end : mod;
            } else {
                // Decreasing range
                int mod = start + (steps * -1);
                return mod < end ? end : mod;

            }

        }

        // Count the total distance a range will extend
        public static int HexesInRange(int start, int end, int step) {
            
            int range = 0;
            if (start < 0 && end < 0 || start > 0 && end > 0) {
                range = start - end;
            } else if (start > 0) {
                range = start + Math.Abs(end);
            } else {
                range = Math.Abs(start) + end;
            }
            range = Math.Abs(range);
            return range * step;
        }
    }

}
