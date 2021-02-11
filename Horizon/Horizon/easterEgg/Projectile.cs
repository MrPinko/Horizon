using SkiaSharp;
using System.Collections.Generic;
using Xamarin.Essentials;

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

		private float velocity = 2.5f;
		public bool update()
		{
			y -= 10 * velocity;
			
			for(int i= 0; i<enemies.Count; i++)
			{
				if(enemies[i].hitbox().IntersectsWith(SKRect.Create(x, y, width, height)))
				{
					enemies[i].hitted();
					return false;						//elimina il proiettile
				}
			}

			if (y <= 0)
				return false;
			else
				return true;

			
		}
	}
}
