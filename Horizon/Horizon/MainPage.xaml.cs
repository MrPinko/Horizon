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
		private List<Planet> pls3D;
		private Location location;
		public bool stopTimer2D = false;
		public bool stopTimer3D = false;
		private bool isLocationLoaded = false;
		int cont = 0;
		QuaternionSucks rotor;

		static double height = DeviceDisplay.MainDisplayInfo.Height;
		static double width = DeviceDisplay.MainDisplayInfo.Width;

		public MainPage(List<Planet> pls2D, List<Planet> pls3D, Location location)
		{
			NavigationPage.SetHasBackButton(this, false);   //dopo che arrivi alla home non puoi più tornare indietro
															//perchè la pagina precedente è il caricamento, che è la pagina iniziale
			InitializeComponent();

			optionsBtn.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.options.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn2D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn2D.png", typeof(MainPage).GetTypeInfo().Assembly);
			btn3D.ImageSource = ImageSource.FromResource("Horizon.Assets.MenuButton.btn3D.png", typeof(MainPage).GetTypeInfo().Assembly);

			this.pls3D = pls3D;
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
				camera3d = new Camera3D(this, pls3D, (float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
			}

			camera2d = new Camera2D(this, pls2D, height, width, "image");
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
				camera3d = new Camera3D(this, pls3D, (float)sidTime.getSiderealTimeFromLongitude(location.Longitude), (float)location.Latitude, (int)height, (int)width, "image");
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
				cont++;
				if (cont % 120 == 0)
					System.Diagnostics.Debug.WriteLine(giroscope.num+"\n");
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
