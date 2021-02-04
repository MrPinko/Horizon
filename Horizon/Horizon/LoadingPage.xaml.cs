using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Horizon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {

        private Location location = null;
        private float loadingPercentageValue;
        private float loadingPercentage
        {
            get { return loadingPercentageValue; }
            set { loadingPercentageValue = value; loadingBar.ProgressTo(value, 500, Easing.SinOut); }
        }

        private List<Planet> planets2D = new List<Planet>();
        private List<Planet> planets3D = new List<Planet>();
        private readonly string[] planetNames = { "sun", "earth", "moon", "mercury", "venus", "mars", "jupiter", "saturn", "uranus", "neptune" };

        public LoadingPage()
        {
            InitializeComponent();
            loadingPercentage = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            loadStuff();
        }

        public async void loadStuff()
        {
            //carico i pianeti
            for (int i = 0; i < planetNames.Length; i++)
            {
                if (planetNames[i].Equals( "earth" )) //se c'è la terra la metto vuota nell'array
                {
                    planets2D.Add(new Planet(planetNames[i], 0, 0, 0, 0));
                    planets3D.Add(new Planet(planetNames[i], 0, 0, 0, 0));
                    continue;
                }
                loadingLabel.Text = planetNames[i][0].ToString().ToUpper() + planetNames[i].Substring(1) + "...";
                loadingPercentage = ((float)i+1) / planetNames.Length * 0.7f;
                await Task.Run(() =>
                {
                    PlanetRaw pr = Request.getPlanet(planetNames[i]).Result;
                    planets2D.Add(new Planet(planetNames[i], pr));
                    planets3D.Add(new Planet(planetNames[i], pr));
                });
            }

            
            loadingPercentage = ((float)planetNames.Length+2)/planetNames.Length * 0.7f;
            loadingLabel.Text = "Geolocation" + "...";
			//carico la geolocalizzazione
			try{
                Task<Location> taskGl = Geolocation.GetLocationAsync();
                await taskGl.ContinueWith(x =>
                {
                    location = x.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
                loadingPercentage = 1;
                //passo alla pagina con il menu
                await Navigation.PushModalAsync(new MainPage(planets2D, planets3D, location));
			}
			catch {
                await Navigation.PushModalAsync(new MainPage(planets2D, planets3D, null));
            }
        }

    }
}
