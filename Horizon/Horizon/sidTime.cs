using System;

namespace Horizon
{
	public static class sidTime
	{
		//longitude è in gradi
		public static double getSiderealTimeFromLongitude(double longitude)
		{
			var jd = DateTime.UtcNow.ToOADate() + 2415018.5;

			var GMST = GM_Sidereal_Time(jd);
			var LMST = 24.0 * frac((GMST + longitude / 15.0) / 24.0);
			return HoursMinutesSecondsToDegrees(LMST);
		}


		private static double GM_Sidereal_Time(double jd)
		{
			var MJD = jd - 2400000.5;
			var MJD0 = Math.Floor(MJD);
			var ut = (MJD - MJD0) * 24.0;
			var t_eph = (MJD0 - 51544.5) / 36525.0;
			return (6.697374558 + 1.0027379093 * ut + (8640184.812866 + (0.093104 - 0.0000062 * t_eph) * t_eph) * t_eph / 3600.0);
		}


		private static double frac(double X)
		{
			X = X - Math.Floor(X);
			if (X < 0) X = X + 1.0;
			return X;
		}

		private static double HoursMinutesSecondsToDegrees(double time)
		{
			var h = Math.Floor(time);
			var min = Math.Floor(60.0 * frac(time));
			var secs = Math.Round(60.0 * (60.0 * frac(time) - min));

			return (h * 15 + min * 0.25 + secs * 0.25 / 60);
		}

	}
}
