using SkiaSharp;
using System;

namespace Horizon
{
    static class Misc
    {
		public static void println(Object s)
        {
			System.Diagnostics.Debug.WriteLine(s);
        }

        public static float toRad(double x)
        {
            return (float)(x / 180 * Math.PI);
        }

        public static float toDeg(float x)
        {
            return (float)(x * 180 / Math.PI);
        }

		//? è una variabile nullable
		public static SKPoint? Intersects(SKPoint a1, SKPoint a2, SKPoint b1, SKPoint b2)
		{
			SKPoint b = a2 - a1;
			SKPoint d = b2 - b1;

			var bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp == 0)
				return null;

			SKPoint c = b1 - a1;
			var t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
			if (t < 0 || t > 1)
				return null;

			var u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
			if (u < 0 || u > 1)
				return null;

			return a1 + new SKPoint(t * b.X, t * b.Y);
		}
	}
}
