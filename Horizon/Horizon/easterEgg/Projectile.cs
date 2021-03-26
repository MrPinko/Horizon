using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Horizon
{
	class Projectile
	{

		private float height = 10, width = 5;
		private float x, y;
		List<Enemy> enemies;
		SKRect rect;

		public Projectile(float x, float y, List<Enemy> enemies)
		{
			this.x = x;
			this.y = y;
			this.enemies = enemies;
		}

		public void draw(SKCanvas canvas)
		{
			rect = SKRect.Create(x, y, width, height);
			canvas.DrawRect(rect, new SKPaint
			{
				Color = SKColors.White
			});
		}

		private int velocity = 2;
		public bool update()
		{
			y -= 10 * velocity;

			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].hitbox().IntersectsWith(SKRect.Create(x, y, width, height)))
				{
					enemies[i].hitted(1);
					return false;                       //elimina il proiettile
				}
			}

			if (y <= 0)
				return false;
			else
				return true;


		}

	}


	class Rocket
	{

		private float height = 50, width = 50;
		private float x, y;
		List<Enemy> enemies;
		SKRect rect;
		SKBitmap texture;
		int position = -1;		//left

		public Rocket(float x, float y, int position, List<Enemy> enemies)
		{
			this.x = x;
			this.y = y;
			this.enemies = enemies;
			this.position = position;
			setTexture();
		}

		public void draw(SKCanvas canvas)
		{
			rect = SKRect.Create(x-25 + 50*position, y + 50, width, height);
			canvas.DrawBitmap(texture,rect);
		}

		private int velocity = 2;
		private double autoInc = 1;
		public bool update()
		{
			y -= (float)(5 + Math.Exp(autoInc) * velocity);
			autoInc += 0.1;

			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].hitbox().IntersectsWith(SKRect.Create(x - 25 + 50 * position, y + 50, width, height)))
				{
					enemies[i].hitted(5);
					return false;                       //elimina il proiettile
				}
			}

			if (y <= 0)
				return false;
			else
				return true;
		}

		public void setTexture()
		{
			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.missile.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}
		}
	}

}