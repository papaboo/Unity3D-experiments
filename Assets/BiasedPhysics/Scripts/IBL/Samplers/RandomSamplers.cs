using UnityEngine;
using System.Collections;

namespace RandomSamplers {

    public class LowDiscrepency {
        public static void Fill(Vector2[] samples, uint offset, uint scrambleU, uint scrambleV) {
            for (uint i = 0; i < (uint)samples.Length; ++i)
                samples[i] = Utils.Sample02(offset + i, scrambleU, scrambleV);
        }
    }

    public static class Utils {

        const float OneMinusEpsilon = 0.9999999403953552f; // TODO Calculate instead? float(bitpattern(1.0f) - 1)?
    
        public static float VanDerCorput(uint n, uint scramble) {
            // Reverse bits of _n_
            n = (n << 16) | (n >> 16);
            n = ((n & 0x00ff00ff) << 8) | ((n & 0xff00ff00) >> 8);
            n = ((n & 0x0f0f0f0f) << 4) | ((n & 0xf0f0f0f0) >> 4);
            n = ((n & 0x33333333) << 2) | ((n & 0xcccccccc) >> 2);
            n = ((n & 0x55555555) << 1) | ((n & 0xaaaaaaaa) >> 1);
            
            n ^= scramble;
            
            float maxValue = 1 << 24;
            return Mathf.Min(((n>>8) & 0xffffff) / maxValue, OneMinusEpsilon);
        }
        
        public static float Sobol2(uint n, uint scramble) {
            for (uint v = (uint)1 << 31; n != 0; n >>= 1, v ^= v >> 1)
                if ((n & 0x1) != 0) scramble ^= v;
            float maxValue = 1 << 24;
            return Mathf.Min(((scramble>>8) & 0xffffff) / maxValue, OneMinusEpsilon);
        }
        
        public static Vector2 Sample02(uint n, uint scrambleU, uint scrambleV) {
            return new Vector2(VanDerCorput(n, scrambleU),
                               Sobol2(n, scrambleV));
        }

    }
}
