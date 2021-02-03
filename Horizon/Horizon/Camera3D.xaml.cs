﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Numerics;

namespace Horizon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Camera3D : ContentPage
    {
        private MainPage main;
        private List<Planet> planets;
        private List<Planet> stars;

        CustomButton.ChangeTextureButton changeButton;
        CustomButton.SwitchJoyStick switchJoyStick;

        private String theme;
        private List<String> texturepath = new List<string>();
        private List<String> texturepathHD = new List<string>();
        public Boolean useSensor = true;
        public Boolean sensorExists = true;

        private const float baseAmp = 72 / 2; //con 180 gradi ho una visione a 360 gradi (LOL NO) 72 e 90 best imho
        private double RA;
        private double DEC;
        private int width;
        private int height;
        private float ampWidth;
        private float ampHeight;
        private int planetSize = 70;

        private Point panPoint = new Point(0, 0);
        private float tempDEC;
        private float tempRA;
        public float baseX;
        public float baseY;


        SKPaint uselessPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(255, 255, 255)
        };

        public Camera3D()
        {
            InitializeComponent();
        }

        //COSTRUTTORE
        public Camera3D(MainPage main, List<Planet> planets, float RA, float DEC, int height, int width, String theme)    //quel dec è dove guardiamo se guardiamo in alto
        {
            InitializeComponent();
            this.main = main;
            this.planets = new List<Planet>(planets);
            stars = StarDB.getAll();
            this.RA = tempRA = RA;
            this.DEC = tempDEC = DEC;
            this.width = width;
            this.height = height;
            ampWidth = baseAmp / 2;
            ampHeight = ampWidth * height / width;
            this.theme = theme;
            setTexture();
            setTextureHD();
            changeButton = new CustomButton.ChangeTextureButton((float)width, (float)height, 150, 150);
            switchJoyStick = new CustomButton.SwitchJoyStick((float)width, (float)height, 100, 100);
        }

        //BACK
        protected override bool OnBackButtonPressed()
        {
            main.stopTimer3D = true;
            return base.OnBackButtonPressed();
        }

        //BOTTONI
        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            SKRect touchRect = SKRect.Create(e.Location.X, e.Location.Y, 1, 1);

            //cambio il tema dei pianeti
            if (touchRect.IntersectsWith(changeButton.GetRect()))
            {
                if (theme.Equals("image"))
                    theme = "imageHD";
                else
                    theme = "image";
                changeButton.switchTheme();
            }

            //abilito/disabilito giroscopio
            if (touchRect.IntersectsWith(switchJoyStick.GetRect()))
            {
                if(sensorExists)
                {
                    switchJoyStick.changeStateOn();
                    useSensor = !useSensor;
                    if (!useSensor)
                    {
                        tempRA = (float)RA;
                        tempDEC = (float)DEC;
                    }
                }
            }
        }

        //JOISTICK FATTO BENE
        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)  //dito che preme e si muove in giro
        {

            if (useSensor == true)
                return;
            if (e.StatusType.ToString() == "Running")
            {
                panPoint.X = e.TotalX;  //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info
                panPoint.Y = e.TotalY;

                RA = tempRA - (float)panPoint.X / (width / 2) * ampWidth * 5.2f;        //regolare qui la sensibilità
                DEC = tempDEC + (float)panPoint.Y / (height / 2) * ampHeight * 2.6f;

                while (DEC > 90)
                    DEC -= 180;
                while (DEC < -90)
                    DEC += 180;

                while (RA > 360)
                    RA -= 360;
                while (RA < 0)
                    RA += 360;
            }
            else if (e.StatusType.ToString() == "Completed")  //quando il dito finisce di spostarsi il centro della telecamera si muove
            {
                tempDEC = (float)DEC;
                tempRA = (float)RA;
                panPoint.X = 0;
                panPoint.Y = 0;
            }

        }

        //STAMPA
        private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            for (int i = 0; i < stars.Count; i++)  //SCORRO LE STELLE
            {
                canvas.DrawCircle(toScreen(stars[i]), /*stars[i].printSize*/1, stars[i].paint);
            }

            canvas.DrawCircle(toScreen(new Planet("POLOSUD", 0, -90, 10, new SKColor(255, 255, 255))), planetSize, uselessPaint);
            canvas.DrawCircle(toScreen(new Planet("POLONORD", 0, 90, 10, new SKColor(127, 127, 127))), planetSize, uselessPaint);
            for (float i = 0; i <= 360; i = i + 0.2f)
                canvas.DrawCircle(toScreen(new Planet("BINGO", i, 0, 3, new SKColor(0, 127, 127))), 10, uselessPaint);

            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i].name == "earth")
                    continue;

                if (theme.Equals("circle"))
                {
                    planets[i].paint.TextSize = 50;
                    canvas.DrawCircle(toScreen(planets[i]), planetSize, planets[i].paint);
                    canvas.DrawText((planets[i].name), toScreen(planets[i]), planets[i].paint);
                }
                if (theme.Equals("image"))              //disegno i pianeti come immagini stilizzate
                {
                    SKPoint tempPoint = toScreen(planets[i]);
                    tempPoint.X -= (200 + planets[i].printSize * 15) / 2;
                    tempPoint.Y -= (200 + planets[i].printSize * 15) / 2;
                    canvas.DrawBitmap(planets[i].texture, SKRect.Create(tempPoint, new SKSize(200 + planets[i].printSize * 15, 200 + planets[i].printSize * 15)), null);
                }
                if (theme.Equals("imageHD"))            //disegno i pianeti come immagini reali
                {
                    SKPoint tempPoint = toScreen(planets[i]);
                    tempPoint.X -= (200 + planets[i].printSize * 15) / 2;
                    tempPoint.Y -= (200 + planets[i].printSize * 15) / 2;
                    canvas.DrawBitmap(planets[i].textureHD, SKRect.Create(tempPoint, new SKSize(200 + planets[i].printSize * 15, 200 + planets[i].printSize * 15)), null);
                }
            }

            changeButton.draw(canvas);
            if( sensorExists )
                switchJoyStick.draw(canvas);
        }


        //A SCHERMO
        private SKPoint toScreen(Planet body)
        {
            SKPoint cord = new SKPoint(width / 2, height / 2);

            var camDec = toRad(-DEC);
            var camRa = toRad(RA);
            var bodyDec = toRad(-body.DEC);
            var bodyRa = toRad(body.RA);
            var a = Math.Acos((Math.Sin(camDec)) * (Math.Sin(bodyDec)) + (Math.Cos(camDec)) * (Math.Cos(bodyDec)) * (Math.Cos(bodyRa - camRa)));
            var b = a / Math.Sin(a);

            var deltaX = (float)(b * (Math.Cos(bodyDec)) * (Math.Sin(bodyRa - camRa))) * width;
            var deltaY = (float)(b * ((Math.Cos(camDec)) * (Math.Sin(bodyDec)) - (Math.Sin(camDec)) * (Math.Cos(bodyDec)) * (Math.Cos(bodyRa - camRa)))) * width;
            
            if (useSensor)
            {
                /*var diag = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                var c = Math.Atan2(deltaY, deltaX) + toRad((float)main.giroscope.pitch);
                cord.X += (float)(Math.Cos(c) * diag);
                cord.Y += (float)(Math.Sin(c) * diag);*/

                cord.X += deltaX;
                cord.Y += deltaY;
            }
            else
            {
                cord.X += deltaX;
                cord.Y += deltaY;
            }

            return cord;
        }

        public float toRad(double x)
        {
            return (float)(x / 180 * Math.PI);
        }

        public float toDeg(float x)
        {
            return (float)(x * 180 / Math.PI);
        }

        //GIROSCOPIO
        public void updateFromSensor()  //base = se guardasse precisamente in alto  
        {
            RA = toDeg((float)main.giroscope.yaw);
            DEC = toDeg((float)main.giroscope.roll);
            //rotor.update(main.giroscope.yaw, main.giroscope.roll);
            //this.RA = toDeg((float)rotor.RA);
            //this.DEC = toDeg((float)rotor.DEC);
            //System.Diagnostics.Debug.WriteLine("RA: " + RA + "\nDEC: " + DEC);
        }
        int lessLag = 0;
        //COSE DA LOOPARE
        public void loop()
        {
            if ( useSensor == true && ++lessLag == 6)
            {
                updateFromSensor();
                lessLag = 0;
            }

            canvasView.InvalidateSurface();
        }

        private void setTexture()
        {
            loadTexture();
            for (int i = 0; i < texturepath.Count; i++)
            {
                planets[i].setTexture(texturepath[i]);
            }
            
        }

        private void loadTexture()
        {
            texturepath.Add("Horizon.Assets.Image.sun.png");
            texturepath.Add("Horizon.Assets.Image.earth.png");
            texturepath.Add("Horizon.Assets.Image.moon.png");
            texturepath.Add("Horizon.Assets.Image.mercury.png");
            texturepath.Add("Horizon.Assets.Image.venus.png");
            texturepath.Add("Horizon.Assets.Image.mars.png");
            texturepath.Add("Horizon.Assets.Image.jupiter.png");
            texturepath.Add("Horizon.Assets.Image.saturn.png");
            texturepath.Add("Horizon.Assets.Image.uranus.png");
            texturepath.Add("Horizon.Assets.Image.neptune.png");
        }

        private void setTextureHD()
        {
            loadTextureHD();
            for (int i = 0; i < texturepathHD.Count; i++)
            {
                planets[i].setTextureHD(texturepathHD[i]);
            }
        }
        private void loadTextureHD()
        {
            texturepathHD.Add("Horizon.Assets.ImageHD.sun.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.earth.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.moon.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.mercury.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.venus.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.mars.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.jupiter.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.saturn.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.uranus.png");
            texturepathHD.Add("Horizon.Assets.ImageHD.neptune.png");
        }

    }
}
