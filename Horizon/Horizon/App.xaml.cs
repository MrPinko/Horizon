using Xamarin.Forms;

namespace Horizon
{
	public partial class App : Application
	{
		LoadingPage lp;
		public App()
		{
			InitializeComponent();
			

			lp = new LoadingPage();
			MainPage = lp;
		}

		protected override void OnStart()
		{

		}

		protected override void OnSleep()
		{

		}

		protected override void OnResume()
		{

		}
		
	}
}
