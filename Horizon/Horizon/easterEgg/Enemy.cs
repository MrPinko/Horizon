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


		private float height = 50, width = 50;
		private float x, y;
		private int health = 5;
		private bool isHitted = false;
		private SKCanvas canvas;
		private List<Enemy> enemies;
		private SKPaint paint = new SKPaint();
		SKBitmap texture, textureRed;

		public Enemy(float x, float y, List<Enemy> enemies)
		{
			this.x = x;
			this.y = y;
			this.enemies = enemies;
			setTexture();
		}

		public void draw(SKCanvas canvas)
		{
			if (!isHitted)
				canvas.DrawBitmap(texture, SKRect.Create(x, y, width, height), paint);
			else
			{
				canvas.DrawBitmap(textureRed, SKRect.Create(x, y, width, height), paint);
				isHitted = false;
			}

		}

		double autoInc = 0, autoInc2 = 0;
		bool change = false;


		public bool update()
		{

			if (change)
			{
				y += trueHeight / 5;
				if (y > trueHeight - 50)
					return false;
				change = false;
			}

			if (autoInc2 % (10 + enemies.Count) == 0)
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

		public void hitted(int damage)
		{
			health -= damage;
			isHitted = true;

			if (health <= 0)
			{
				enemies.Remove(this);
				if (!EasterEgg.isRocketPowerUp)
					dropChance();
			}
		}

		public void dropChance()
		{
			Random random = new Random();
			//if (random.NextDouble() <= 0.2)
			//{
				EasterEgg.listPowerup.Add(new DropPowerUp(x, y));
			//}
		}

		public void setTexture()
		{
			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.spaceShip.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}

			stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.spaceShipRed.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				textureRed = SKBitmap.Decode(skStream);
			}
		}
	}
}