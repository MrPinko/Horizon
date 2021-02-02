using System;
using Xamarin.Forms;
using SkiaSharp;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Horizon
{
    public class Planet
    {
        public string name;
        public Point coord;
        public float RA;
        public float DEC;
        public float dist2D;
        public float sunDist = 1;

        public float distanceKm;
        public float distanceLight;

        public float printSize;
        public float Size;

        public Point originalCoord;
        public double orbitRateo;

        public List<double> planetData = new List<double>();
        public List<String> planetDataString = new List<string>();


        public SKPaint paint;
        public SKBitmap texture, textureHD;

        public Planet(string name, PlanetRaw pr)
        {
            this.name = name;
            coord = new Point(0, 0);
            RA = (float)toDouble(pr.rows[0][1]);
            DEC = (float)toDouble(pr.rows[0][2]);
            distanceKm = (float)toDouble(pr.rows[0][3]);
            distanceLight = (float)toDouble(pr.rows[0][9]);

            dist2D = (float)(Math.Abs(Math.Cos(DEC / 180 * Math.PI)) * distanceKm);

            loadString();
            uselessDataBase.setUselessData(this);
        }

        public Planet(string name, float RA, float DEC, double dim, SKColor color)
        {
            this.name = name;
            this.RA = RA;
            this.DEC = DEC;
            setColor(color);
            printSize = (float)dim;
        }

        public Planet(string name, float RA, float DEC, float distanceKm, float distanceLight)
        {
            this.name = name;
            coord = new Point(0, 0);
            this.RA = RA;
            this.DEC = DEC;
            this.distanceKm = distanceKm;
            this.distanceLight = distanceLight;

            dist2D = (float)(Math.Abs(Math.Cos(DEC / 180 * Math.PI)) * distanceKm);

            loadString();
            uselessDataBase.setUselessData(this);
        }


        public void setColor(SKColor color)
        {
            paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color
            };
        }

        public void setTexture(String path)
        {
            Assembly assembly = typeof(MainPage).Assembly;
            var stream = assembly.GetManifestResourceStream(path);
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                texture = SKBitmap.Decode(skStream);
            }

        }

        public void setTextureHD(String path)
        {
            Assembly assembly = typeof(MainPage).Assembly;
            var stream = assembly.GetManifestResourceStream(path);
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                textureHD = SKBitmap.Decode(skStream);
            }

        }

        public SKRect hitBox(double x, double y, float dim, float scale)
        {
            SKRect rect = SKRect.Create((float)x - (Size*scale) / 2, (float)y - (Size*scale) /2 , dim * scale, dim * scale);

            return rect;
        }

        public void loadString()
        {
			planetDataString.Add("Radius");
            planetDataString.Add("Surface");        //km
            planetDataString.Add("Volume");     //m^3
            planetDataString.Add("Mass");        //kg
            planetDataString.Add("Density");         //g/cm^3
            planetDataString.Add("Gravity");            //m/s^2
            planetDataString.Add("Orbital P.");          //anni orbita attorno al sole
            planetDataString.Add("Rotation P.");             //giorni rotazione
            planetDataString.Add("Moon");                  //numero lune
            planetDataString.Add("Temp Min");    //celsius
            planetDataString.Add("Temp Max");            //celsius
            planetDataString.Add("Elements");       //3 elementi chimici più presenti
        }

        private static double toDouble(string s)      //ovviamente va usata la virgola e non il punto per formattare un Double ...
        {
            double d = 0;
            bool isNegative = false;
            //leggo la parte prima del punto
            int i = 0;
            int decimalIndex = 0;
            StringBuilder b = new StringBuilder("");

            if (s[0] == '-')
            {
                isNegative = true;
                i++;
            }

            for (; i < s.Length; i++)
            {
                if (s[i] == '.')
                {
                    decimalIndex = i;
                    i++;
                    break;
                }
                d *= 10;
                d += toInt(s[i]);
            }
            for (; i < s.Length; i++)
            {
                if (s[i] == 'E' || s[i] == 'e')
                {
                    i++;
                    break;
                }
                d += toInt(s[i]) * Math.Pow(10, -(i - decimalIndex));
            }
            for (; i < s.Length; i++)
            {
                b.Append(s[i]);
            }
            if (b.ToString() != "")
            {
                int n = Convert.ToInt32(b.ToString());
                d *= Math.Pow(10, n);
            }
            if (isNegative)
                d = -d;

            return d;
        }

        private static int toInt(char c)
        {
            return (Convert.ToInt32(c) - 48);
        }
    }

}
