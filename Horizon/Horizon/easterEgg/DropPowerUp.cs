using System.Reflection;
using SkiaSharp;

namespace Horizon
{
	class DropPowerUp
	{
		float x, y;
		int textureWidth = 25, textureHeight = 25;
		SKBitmap texture;

		public DropPowerUp(float x, float y)
		{
			this.x = x;
			this.y = y;
			setTexture();
		}

		public void draw(SKCanvas canvas)
		{
			canvas.DrawBitmap(texture, hitbox());
		}

		public bool update(EasterEgg easterEgg)
		{
			if (hitbox().IntersectsWith(easterEgg.hitbox()))
				return false;

			y += 20;
			
			return true;
		}

		public SKRect hitbox()
		{
			return SKRect.Create(x + textureWidth, y + textureHeight, 50, 50);
		}

		public void setTexture()
		{
			Assembly assembly = typeof(MainPage).Assembly;
			var stream = assembly.GetManifestResourceStream("Horizon.Assets.Rocket.powerup.png");
			using (SKManagedStream skStream = new SKManagedStream(stream))
			{
				texture = SKBitmap.Decode(skStream);
			}

		}
	}
}