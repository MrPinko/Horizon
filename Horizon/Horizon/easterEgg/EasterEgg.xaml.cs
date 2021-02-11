using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Horizon
{
	public partial class EasterEgg : ContentPage
	{
		public EasterEgg()
		{
			InitializeComponent();
			setTexture();
			x = width / 2;
			y = height - height / 6; 

			for(int i = 1; i< 12; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					enemyList.Add(new Enemy(width / 12 * i, height / 12 *j, enemyList));
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
		private bool defeat = false;

		static float height = (float)DeviceDisplay.MainDisplayInfo.Height;
		static float width = (float)DeviceDisplay.MainDisplayInfo.Width;
		private float textureWidth = 100 , textureHeight = 100;
		private List<Projectile> list = new List<Projectile>();
		private List<Enemy> enemyList = new List<Enemy>();
		private SKBitmap texture;

		private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Black);

			if (!defeat)
			{
				canvas.DrawBitmap(texture, SKRect.Create(x + translate, y, textureWidth, textureHeight), new SKPaint { Color = SKColors.Red });

				foreach (Enemy enemy in enemyList)
				{
					if (enemy.hitbox().IntersectsWith(SKRect.Create(x + translate, y, textureWidth, textureHeight)))
						{
						defeat = true;
					}
				}

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

		private void Button_Pressed(object sender, EventArgs e)
		{
			(Application.Current).MainPage = new EasterEgg();
		}

		private bool right = false, left = false;


		private void canvasView_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
		{
			if (e.ActionType == SkiaSharp.Views.Forms.SKTouchAction.Pressed)
			{
				if (e.Location.X >= width / 2)
					right = true;
				else
					left = true;
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
			restartButton.Source = ImageSource.FromResource("Horizon.Assets.Rocket.replay.png", typeof(Camera3D).GetTypeInfo().Assembly);


			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.rocketLaunch.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}

		}

		//BACK
		protected override bool OnBackButtonPressed()
		{
			return base.OnBackButtonPressed();
		}

	}
}
