using System;
using System.Numerics;
using Xamarin.Essentials;

namespace Horizon
{
    public class OrientationSensorTest
    {
        // Set speed delay for monitoring changes.
        SensorSpeed speed = SensorSpeed.UI;
        public double yaw;
        public double pitch;
        public double roll;
        public int num = 0;

        public OrientationSensorTest()
        {
            OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
        }

        void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
        {
            //c++;                    //c'è questa cosa perchè i sensori fanno una misurazione circa ogni 10ms,
            //if (c % 10 == 0)        //quindi spamma troppo e il garbage collector esplode
            //return;             //facendo così 9 misurazioni su 10 le skippa e quindi è più leggero
            //if (c == 1000000)
            //c = 0;
            num++;
            ToEulerAngles(e.Reading.Orientation);
        }

        public bool ToggleOrientationSensor()
        {
            try
            {
                if (OrientationSensor.IsMonitoring)
                    OrientationSensor.Stop();
                else
                    OrientationSensor.Start(speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                return false;
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                return false;
                // Other error has occurred.
            }
            return true;
        }

        void ToEulerAngles(Quaternion q)
        {
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            roll = (float)System.Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                pitch = (float)CopySign(Math.PI / 2, sinp);
            else
                pitch = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            // li aggiusto
            yaw = -yaw;
            roll = (roll - Math.PI/2);
            pitch = -pitch;

        }

        double CopySign(double x, double y)
        {
            return Math.Abs(x) * y / Math.Abs(y);
        }
    }
}
