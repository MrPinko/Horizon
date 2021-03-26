using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Horizon
{
	public partial class EasterEgg : ContentPage
	{
		internal static List<DropPowerUp> listPowerup = new List<DropPowerUp>();

		public EasterEgg()
		{
			InitializeComponent();
			setTexture();
			x = width / 2;
			y = height - height / 6;

			for (int i = 1; i < 12; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					enemyList.Add(new Enemy(width / 12 * i - 50, height / 10 * j, enemyList));
				}
			}
			
			loop();

		}

		private void loop()
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(32), () =>
			{

				canvasView.InvalidateSurface();

				return true;
			});
		}

		private int count = 0;
		private float x = 0, y;
		private int translate = 0, velocity = 2;
		public bool defeat = false;
		public static bool isRocketPowerUp = false;
		public int rocketPosition = 1;

		static float height = (float)DeviceDisplay.MainDisplayInfo.Height;
		static float width = (float)DeviceDisplay.MainDisplayInfo.Width;
		private float textureWidth = 100, textureHeight = 100;
		private List<Projectile> list = new List<Projectile>();
		private List<Rocket> rocketList = new List<Rocket>();

		private List<Enemy> enemyList = new List<Enemy>();
		private SKBitmap texture;

		private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Black);

			if (!defeat)
			{
				canvas.DrawBitmap(texture, SKRect.Create(x + translate, y, textureWidth, textureHeight), new SKPaint { Color = SKColors.Red });

				if (right)
				{
					if (x + translate + 10 * velocity + textureWidth <= width)
						translate += 10 * velocity;
				}
				else if (left)
				{
					if (x + translate - 10 * velocity >= 0)
						translate -= 10 * velocity;
				}
				if (count % 5 == 0)
					list.Add(new Projectile(x + translate + textureWidth / 2, y, enemyList));

				if (isRocketPowerUp)
					if (count % 20 == 0)
					{
						rocketPosition *= -1;             //alterna razzo a destra e sinistra
						rocketList.Add(new Rocket(x + translate + textureWidth / 2, y, rocketPosition, enemyList));
					}

				count++;

				for (int i = 0; i < list.Count; i++)
				{
					list[i].draw(canvas);
					if (!list[i].update())
					{
						list.RemoveAt(i);
						i--;
					}
				}

				if(listPowerup.Count != 0)
				{
					for (int i = 0; i < listPowerup.Count; i++)
					{
						listPowerup[i].draw(canvas);
						if (!listPowerup[i].update(this))
						{
							isRocketPowerUp = true;
							listPowerup.RemoveAt(i);
							i--;
						}
					}
				}

				if (isRocketPowerUp)
					for (int i = 0; i < rocketList.Count; i++)
					{
						rocketList[i].draw(canvas);
						if (!rocketList[i].update())
						{
							rocketList.RemoveAt(i);
							i--;
						}
					}

				for (int i = 0; i < enemyList.Count; i++)
				{
					enemyList[i].draw(canvas);
					if (!enemyList[i].update())
					{
						defeat = true;
					}
				}
			}
			else
			{
				defeatText.IsVisible = true;
				restartButton.IsVisible = true;
			}
		}

		private bool right = false, left = false;


		private void restartButton_Pressed(object sender, EventArgs e)
		{
			Navigation.PopModalAsync();
			Navigation.PushModalAsync(new EasterEgg());
			isRocketPowerUp = false;
		}

		private void canvasView_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
		{
			if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
			{
				if (e.Location.X >= width / 2)
				{
					right = true;
					left = false;
				}
				else
				{
					right = false;
					left = true;
				}
			}

			else if (e.ActionType == SkiaSharp.Views.Forms.SKTouchAction.Released)
			{
				right = false;
				left = false;
			}

			e.Handled = true;
		}

		public void setTexture()
		{
			restartButton.Source = ImageSource.FromResource("Horizon.Assets.Rocket.replay.png", typeof(EasterEgg).GetTypeInfo().Assembly);

			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.rocketLaunch.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}

		}

		public SKRect hitbox()
		{
			return SKRect.Create(x + translate, y, textureWidth, textureHeight);
		}


		protected override bool OnBackButtonPressed()
		{
			return base.OnBackButtonPressed();
		}

	}
}