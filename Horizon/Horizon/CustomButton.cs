using System;
using System.Reflection;
using SkiaSharp;

namespace Horizon
{
    public class CustomButton
    {
        SKRect rect;

        public SKRect GetRect()      //ritorna il rettangolo in cui è disegnata l'immagine (comoda per le hitbox)
        {
            return rect;
        }

        public class ChangeTextureButton : CustomButton               //pulsante per cmbiare il tema dei pianeti  (ora disponibili immagini stilizzate e hd other soon..)
        {
            private SKBitmap texture1, texture2;
            Boolean theme1 = true;

            public ChangeTextureButton(float width, float height, float X, float Y)
            {
                rect = new SKRect(width - X, 0, width, Y);
                setTexture();
            }

            public void draw(SKCanvas canvas)
            {
                if (theme1)
                    canvas.DrawBitmap(texture1, rect, null);
                else
                    canvas.DrawBitmap(texture2, rect, null);

            }

            public void switchTheme()
            {
                theme1 = !theme1;
            }

            public void setTexture()
            {
                Assembly assembly = typeof(MainPage).Assembly;
                var stream = assembly.GetManifestResourceStream("Horizon.Assets.CustomButton.Theme1.png");
                using (SKManagedStream skStream = new SKManagedStream(stream))
                {
                    texture1 = SKBitmap.Decode(skStream);
                }

                var stream2 = assembly.GetManifestResourceStream("Horizon.Assets.CustomButton.Theme2.png");
                using (SKManagedStream skStream = new SKManagedStream(stream2))
                {
                    texture2 = SKBitmap.Decode(skStream);
                }
            }
        }

        public class JoyStick : CustomButton            //pulsante joystick
        {
            private SKBitmap texture;
            private float x, y;
            private SKRect top, left, down, right;

            public JoyStick(float width, float height, float x, float y)
            {

                this.x = x;
                this.y = y;
                setTexture();

                rect = new SKRect(width - x, height - y - texture.Height / 2, width, height - texture.Height / 2);
                setHitbox();
            }

            public void draw(SKCanvas canvas)
            {
                canvas.DrawBitmap(texture, rect, null);
            }

            //ogni bottone ha una propria hitbox calcolata scientificamente
            public void setHitbox()
            {
                top = new SKRect(rect.Left + rect.Width / 2 - 50,
                    rect.Top,
                    rect.Right - rect.Width / 2 + 50,
                    rect.Bottom - rect.Height / 2);

                right = new SKRect(rect.Left + rect.Width / 2,
                    rect.Top + rect.Height / 2 - 50,
                    rect.Right,
                    rect.Bottom - rect.Height / 2 + 50);

                down = new SKRect(rect.Left + rect.Width / 2 - 50,
                     rect.Top + rect.Height / 2,
                     rect.Right - rect.Width / 2 + 50,
                     rect.Bottom);

                left = new SKRect(rect.Left,
                    rect.Top + rect.Height / 2 - 50,
                    rect.Right - rect.Width / 2,
                    rect.Bottom - rect.Height / 2 + 50);
            }

            public void setTexture()
            {
                Assembly assembly = typeof(MainPage).Assembly;
                var stream = assembly.GetManifestResourceStream("Horizon.Assets.CustomButton.joystick.png");
                using (SKManagedStream skStream = new SKManagedStream(stream))
                {
                    texture = SKBitmap.Decode(skStream);
                }
            }

            //ritorno i vari rettangoli creati per ogni bottone
            public SKRect getTop()
            {
                return top;
            }
            public SKRect getRight()
            {
                return right;
            }
            public SKRect getDown()
            {
                return down;
            }
            public SKRect getleft()
            {
                return left;
            }

        }

        public class SwitchJoyStick : CustomButton            //pulsante per abilitare/disabilitare joistick
        {
            private SKBitmap texture, texture2;
            Boolean on = false;     //mostrare joystick
            private (float, float, int, int) p;

            public SwitchJoyStick(float width, float height, float X, float Y)
            {

                setTexture();

                rect = new SKRect(width - X - 25, 200, width - 25, 200 + Y);

            }

            public void draw(SKCanvas canvas)
            {
                if (on)        //cambio della texture in base allo stato del joystick
                {
                    canvas.DrawBitmap(texture2, rect, null);
                }
                else
                {
                    canvas.DrawBitmap(texture, rect, null);
                }

            }

            public void changeStateOn()
            {
                on = !on;
            }

            public void setTexture()
            {

                Assembly assembly = typeof(MainPage).Assembly;
                var stream = assembly.GetManifestResourceStream("Horizon.Assets.CustomButton.controllerSwitch.png");
                using (SKManagedStream skStream = new SKManagedStream(stream))
                {
                    texture = SKBitmap.Decode(skStream);
                }

                var stream2 = assembly.GetManifestResourceStream("Horizon.Assets.CustomButton.controllerSwitch2.png");
                using (SKManagedStream skStream = new SKManagedStream(stream2))
                {
                    texture2 = SKBitmap.Decode(skStream);
                }
            }
        }
    }
}
