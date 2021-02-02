using System;

namespace Horizon
{
    static class Ez
    {
        public static float toRad(double x)
        {
            return (float)(x / 180 * Math.PI);
        }

        public static float toDeg(float x)
        {
            return (float)(x * 180 / Math.PI);
        }
    }
}
