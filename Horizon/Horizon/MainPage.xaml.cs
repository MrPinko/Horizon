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

			optionsBtn.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.options150.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn2D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn2D600.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn3D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn3D600.png", typeof(MainPage).GetTypeInfo().Assembly);

			this.pls3D = pls3D;
			this.pls2D = pls2D;
			//this.plsSun = setObserver(plsSun, "sun");
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
				camera3d = new Camera3D(this, this.pls3D, "earth", (float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
			}

			camera2d = new Camera2D(this, this.pls2D, height, width, "image");

			//cameraSun = new Camera3D(this, this.plsSun, "sun", 0, 0, (int)height, (int)width, "image");
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

		private async Task askPermissionAsync()
		{
			var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			System.Diagnostics.Debug.WriteLine("status == PermissionStatus.Granted" + status + "==" + PermissionStatus.Granted);

			if (status == PermissionStatus.Granted)
			{
				try
				{
					var req = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
					location = await Geolocation.GetLocationAsync(req);

					//passo alla pagina con il menu
					camera3d = new Camera3D(this, this.pls3D, "earth", (float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
					isLocationLoaded = true;
				}
				catch (Exception e)
				{
					System.Diagnostics.Debug.WriteLine(e.Message);
				}
			}
            else { 
				await DisplayAlert("", "E' necessaria la geolocalizazione per continuare", "OK");
			}

		}

		protected override bool OnBackButtonPressed()
		{
			return false;
		}

		private async void Button_Pressed_3D(object sender, EventArgs e)
		{
			if(!isLocationLoaded)
            {
				await askPermissionAsync();
				if (!isLocationLoaded)
					return;
			}
			await Navigation.PushModalAsync(camera3d);

			if (!giroscope.ToggleOrientationSensor())
			{
				await DisplayAlert("", "Questo dispositivo non supporta la rilevazione del movimento tramite giroscopio. Nella visuale 3D saranno disponibili solamente i controlli touch.", "OK");
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
			try
			{
				Navigation.PushModalAsync(camera2d);
			}
			catch{ }

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
