using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;


namespace Horizon
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public OrientationSensorTest giroscope = new OrientationSensorTest();
		private Camera2D camera2d;
		private Camera3D camera3d;
		private Camera3D cameraSun;
		private List<Planet> pls3D;
		private List<Planet> pls2D;
		private List<Planet> plsSun;
		private Location location;
		public bool stopTimer2D = false;
		public bool stopTimer3D = false;
		private bool isLocationLoaded = false;
		//QuaternionSucks rotor;

		static double height = DeviceDisplay.MainDisplayInfo.Height;
		static double width = DeviceDisplay.MainDisplayInfo.Width;

		public MainPage(List<Planet> pls2D, List<Planet> pls3D, List<Planet> plsSun, Location location)
		{
			NavigationPage.SetHasBackButton(this, false);   //dopo che arrivi alla home non puoi più tornare indietro
															//perchè la pagina precedente è il caricamento, che è la pagina iniziale
			InitializeComponent();

			optionsBtn.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.options.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn2D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn2D.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn3D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn3D.png", typeof(MainPage).GetTypeInfo().Assembly);

			this.pls3D = pls3D;
			this.pls2D = pls2D;
			this.plsSun = setObserver(plsSun, "sun");
			this.location = location;
			/*
			rotor = new QuaternionSucks((float)location.Latitude, (float)sidTime.getSiderealTimeFromLongitude(location.Longitude));
			for( int i=0; i<pls3D.Count; i++ )
            {
				rotor.update(pls3D[i].RA, pls3D[i].DEC);
				pls3D[i].RA  = (float)(rotor.RA  / Math.PI * 180);
				pls3D[i].DEC = (float)(rotor.DEC / Math.PI * 180);
			}
			*/

			if (location != null)
            {
				isLocationLoaded = true;
				camera3d = new Camera3D(this, this.plsSun, "sun", (float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
			}

			camera2d = new Camera2D(this, this.pls2D, height, width, "image");

			cameraSun = new Camera3D(this, this.plsSun, "sun", 0, 0, (int)height, (int)width, "image");
		}
		
		private List<Planet> setObserver(List<Planet> planets, String observer)
        {
			int observerIndex = -1;                             //indice del nuovo osservatore
			float Hyp, tempx, tempy, tempz;

			for (int i = 0; i < planets.Count; i++)			//trovo l'indice del nuovo osservatore
                if (planets[i].name.Equals(observer))
                {
					observerIndex = i;
					continue;
                }
			if (observerIndex == -1)						//se non è stato trovato l'osservatore non faccio alcuna modifica
				return planets;

			for (int i = 0; i < planets.Count; i++)			//trasformo RA e DEC in coordinate vettoriali tenendo conto della distanza dall'osservatore iniziale
            {
				planets[i].x = (float)(planets[i].distanceKm * Math.Cos(Misc.toRad(planets[i].DEC)) * Math.Cos(Misc.toRad(planets[i].RA)));
				planets[i].y = (float)(planets[i].distanceKm * Math.Cos(Misc.toRad(planets[i].DEC)) * Math.Sin(Misc.toRad(planets[i].RA)));
				planets[i].z = (float)(planets[i].distanceKm * Math.Sin(Misc.toRad(planets[i].DEC)));
			}

			tempx = planets[observerIndex].x;				//le salvo in delle variabili perchè poi vengono modificate
			tempy = planets[observerIndex].y;
			tempz = planets[observerIndex].z;
			for (int i = 0; i < planets.Count; i++)         //sottraggo (traslo) le coordinate del nuovo osservatore
			{
				planets[i].x -= tempx;
				planets[i].y -= tempy;
				planets[i].z -= tempz;
			}

			for (int i = 0; i < planets.Count; i++)         //trasformo le coordinate vettoriali in RA e DEC
			{
				Hyp = (float)Math.Sqrt(planets[i].x * planets[i].x + planets[i].y * planets[i].y);		//converto in RA e DEC
				planets[i].RA = Misc.toDeg((float)Math.Atan2(planets[i].y, planets[i].x));    
				planets[i].DEC = Misc.toDeg((float)Math.Atan2(planets[i].z, Hyp));
			}

			return planets;
        }

		private void OptionsPressed(object sender, EventArgs e)
        {
			OpenAppSettings();
        }

		public void OpenAppSettings()	//apro le impostazioni dell'app (non funziona)
		{
			var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
			intent.AddFlags(ActivityFlags.NewTask);
			var uri = Android.Net.Uri.FromParts("package", "Horizon", null);
			intent.SetData(uri);
            Android.App.Application.Context.StartActivity(intent);
		}

		private async System.Threading.Tasks.Task askPermissionAsync()
		{
			var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			if(status == PermissionStatus.Granted)
			{
				Task<Location> taskGl = Geolocation.GetLocationAsync();
				await taskGl.ContinueWith(x =>
				{
					location = x.Result;
				}, TaskScheduler.FromCurrentSynchronizationContext());

				//passo alla pagina con il menu
				camera3d = new Camera3D(this, this.plsSun, "sun",(float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
				isLocationLoaded = true;
			}
			else
				DisplayAlert("", "E' necessaria la geolocalizazione per continuare", "OK");

		}

		protected override bool OnBackButtonPressed()
		{
			return false;
		}

		private void Button_Pressed_3D(object sender, EventArgs e)
		{
			if(!isLocationLoaded)
            {
				askPermissionAsync();
				return;
			}
			Navigation.PushModalAsync(camera3d);

			if (!giroscope.ToggleOrientationSensor())
			{
				DisplayAlert("", "Questo dispositivo non supporta la rilevazione del movimento tramite giroscopio. Nella visuale 3D saranno disponibili solamente i controlli touch.", "OK");
				camera3d.sensorExists = false;
				camera3d.useSensor = false;
			}

			Device.StartTimer(TimeSpan.FromMilliseconds(17), () =>
			{
				if (stopTimer3D)
				{
					giroscope.ToggleOrientationSensor();
					stopTimer3D = false;
					return false;
				}
				camera3d.loop();
				return true;
			});
		}
		
		private void Button_Pressed_2D(object sender, EventArgs e)
		{

			Navigation.PushModalAsync(camera2d);

			Device.StartTimer(TimeSpan.FromMilliseconds(17), () =>
			{
				if (stopTimer2D)
				{
					camera2d.skipIncrement = 0;
					camera2d.timeSkip = 0;
					stopTimer2D = false;
					return false;
				}
				//movimento pianeti 
				if (camera2d.timeIsMoving && camera2d.skipIncrement != 0)
				{
					camera2d.timeSkip += camera2d.skipIncrement;
					camera2d.timeChanged();
				}
				camera2d.loop();
				return true;
			});
			
		}

	}
}
