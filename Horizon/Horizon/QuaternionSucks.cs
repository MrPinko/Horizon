using System;
using System.Numerics;

namespace Horizon
{
    class QuaternionSucks
    {
        public double RA = 0, DEC = 0;
        public double latitude = toRad(0), longitude = toRad(0);

        public QuaternionSucks( double latitude, double longitude )
        {
            this.latitude = toRad(latitude);
            this.longitude = toRad(longitude);
        }

        public void update(double tRA, double tDEC)
        {
            double r = 1;
            //converto
            Vector3 p = new Vector3((float)(r * Math.Cos(tDEC) * Math.Cos(tRA)),
                                    (float)(r * Math.Cos(tDEC) * Math.Sin(tRA)),
                                    (float)(r * Math.Sin(tDEC)));

            p = vertical(p, Math.PI / 2 - latitude);    //spostamento verticale
            p = horizontal(p, longitude);               //spostamento orizzontale

            //converto

            DEC = Math.Asin(p.Z / r);
            RA = Math.Atan2(p.Y, p.X);
            if (RA < 0)
            {
                RA += 2 * Math.PI;
            }

        }

        public Vector3 horizontal(Vector3 p, double a)
        {
            return new Vector3((float)(p.X * Math.Cos(a) - p.Y * Math.Sin(a)),
                               (float)(p.X * Math.Sin(a) + p.Y * Math.Cos(a)),
                               p.Z);
        }

        public Vector3 vertical(Vector3 p, double a)
        {
            return new Vector3((float)(p.X * Math.Cos(a) + p.Z * Math.Sin(a)),
                               p.Y,
                               (float)(p.Z * Math.Cos(a) - p.X * Math.Sin(a)));
        }

        static public double toRad(double x)
        {
            return (float)(x / 180 * Math.PI);
        }

        static public double toDeg(double x)
        {
            return (float)(x * 180 / Math.PI);
        }
    }
}
