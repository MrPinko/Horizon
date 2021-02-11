using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Essentials;

namespace Horizon
{
	class Enemy
	{
		float trueHeight = (float)DeviceDisplay.MainDisplayInfo.Height;
		float trueWidth = (float)DeviceDisplay.MainDisplayInfo.Width;


		private float height = 75, width = 75;
		private float x, y;
		private int health = 5;
		private List<Enemy> enemies;
		SKBitmap texture;

		public Enemy(float x, float y, List<Enemy> enemies)
		{
			this.x = x;
			this.y = y;
			this.enemies = enemies;
			setTexture();
		}

		public void draw(SKCanvas canvas)
		{

			canvas.DrawBitmap(texture, SKRect.Create(x, y, width, height));
		}

		double autoInc = 0, autoInc2 = 0;
		bool change = false;

		public bool update()
		{

			if(change)
			{
				y += trueHeight / 14;
				if (y > trueHeight)
					return false;
				change = false;
			}

			if (autoInc2 % (7 + enemies.Count) == 0)
			{
				autoInc2 = 0;
				x += (float)Math.Sin(autoInc) * 35;

				if (Math.Sin(autoInc) > 0 && Math.Sin(autoInc + 1) < 0)
				{
					change = true;
				}
				else if (Math.Sin(autoInc) < 0 && Math.Sin(autoInc + 1) > 0)
				{
					change = true;
				}

				autoInc++;
			}
			autoInc2++;

			return true;
		}

		public SKRect hitbox()
		{
			return SKRect.Create(x, y, width, height);
		}


		public void hitted()
		{
			health--;
			if (health <= 0)
				enemies.Remove(this);

		}

		public void setTexture()
		{
			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.spaceShip.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}

		}
	}
}
