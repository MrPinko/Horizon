﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Reflection;
using Xamarin.Essentials;
using System.Timers;

namespace Horizon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Camera3D : ContentPage
    {
        private bool startup = true;
        private double dpi = DeviceDisplay.MainDisplayInfo.Density;

        private MainPage main;
        private List<Planet> planets;
        private List<Planet> stars;
        private String observer;

        CustomButton.ChangeTextureButton changeButton;
        CustomButton.SwitchJoyStick switchJoyStick;

        private String theme;
        private List<String> texturepath = new List<string>();
        private List<String> texturepathHD = new List<string>();
        public Boolean useSensor = true;
        public Boolean sensorExists = true;

        private const float baseAmp = 72 / 2; //72 e 90 best imho
        private double RA;
        private double DEC;
        private int width;
        private int height;
        private float ampWidth;
        private float ampHeight;

        private Point panPoint = new Point(0, 0);
        private float tempDEC;
        private float tempRA;
        public float baseX;
        public float baseY;

        private Constellations constellations;

        //paint per le cose inutili (nord, sud, cerchio nell'equatore)
        public SKPaint uselessPaint = new SKPaint{
            Style = SKPaintStyle.Fill,
            Color = new SKColor(255, 255, 255)};

        //paint per le linee delle costellazioni
        public SKPaint constellationPaint = new SKPaint {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(255, 255, 255),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            TextSize = 30 };


        //-------------------------------------------------------------------------------------------------------------------\\
        #region COSE PRINCIPALI

        //COSTRUTTORE
        public Camera3D(MainPage main, List<Planet> planets, String observer, float RA, float DEC, int height, int width, String theme)    //quel dec è dove guardiamo se guardiamo in alto
        {
            InitializeComponent();
            this.main = main;
            this.planets = new List<Planet>(planets);
            stars = StarDB.getAll();
            this.observer = observer;
            this.RA = tempRA = RA;
            this.DEC = tempDEC = DEC;
            this.width = width;
            this.height = height;
            ampWidth = baseAmp / 2;
            ampHeight = ampWidth * height / width;
            this.theme = theme;

            //inizializzazioni
            setTexture();
            setTextureHD();
            loadBottomBarTexture();
            changeButton = new CustomButton.ChangeTextureButton((float)width, (float)height, 150, 150);
            switchJoyStick = new CustomButton.SwitchJoyStick((float)width, (float)height, 100, 100);

            this.constellations = new Constellations(this, stars);
            this.constellations.printText = true;
        }

        //BACK
        protected override bool OnBackButtonPressed()
        {
            main.stopTimer3D = true;
            return base.OnBackButtonPressed();
        }
        
        //STAMPA
        private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            if (startup)
            {
                translateBottonBarDown();              //la barra non c'è
                rocketLabel.TranslateTo(0, -rocketLabelImage.Height * 2 , 0);                     //il dita del razzo non ci sono
                rocketLabelImage.TranslateTo(0, -rocketLabelImage.Height * 2, 0);                     //il razzo non c'è
                startup = false;
            }

            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();

            //stampo le stelle
            for (int i = 0; i < stars.Count; i++)
                canvas.DrawCircle(toScreen(stars[i]), 1, stars[i].paint);

            //stampo le costellazioni
            //SI BUGGA SE METTEREMO LO ZOOM, CHIEDI AL PIETRO
            constellations.drawAll(canvas);
            
            
            //test (sud, nord, equatore)
            canvas.DrawCircle(toScreen(new Planet("SOUTHPOLE", 0, -90, 10, new SKColor(255, 255, 255))), 60, uselessPaint);
            canvas.DrawCircle(toScreen(new Planet("NORTHPOLE", 0, 90, 10, new SKColor(127, 127, 127))), 60, uselessPaint);
            for (float i = 0; i <= 360; i = i + 0.2f)
                canvas.DrawCircle(toScreen(new Planet("EQUATOR", i, 0, 3, new SKColor(0, 127, 127))), 5, uselessPaint);
            
            
            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i].name == observer)  //non stampo la terra o il sole in base dal punto di vista
                    continue;

                SKPoint tempPoint = toScreen(planets[i]);
                //sposto le coordinate di stampa del pianeta in base alla dimensione con cui viene stampato (drawbitmap non disegna partendo dal centro)
                tempPoint.X -= (200 + planets[i].printSize * 15) / 2;
                tempPoint.Y -= (200 + planets[i].printSize * 15) / 2;

                if (theme.Equals("image"))              //disegno i pianeti come immagini stilizzate
                    canvas.DrawBitmap(planets[i].texture, SKRect.Create(tempPoint, new SKSize(200 + planets[i].printSize * 15, 200 + planets[i].printSize * 15)), null);
                else if (theme.Equals("imageHD"))            //disegno i pianeti come immagini reali
                    canvas.DrawBitmap(planets[i].textureHD, SKRect.Create(tempPoint, new SKSize(200 + planets[i].printSize * 15, 200 + planets[i].printSize * 15)), null);
            }

            if (sensorExists)
                switchJoyStick.draw(canvas);
        }


        //loop che viene chiamato dal main
        public void loop()
        {
            if( useSensor ) 
                updateFromSensor();
            canvasView.InvalidateSurface();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------\\
        #region FUNZIONI TOCCO
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

                if (DEC > 90)
                    DEC = 90;
                if (DEC < -90)
                    DEC = -90;

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


        //BOTTONI
        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            SKRect touchRect = SKRect.Create(e.Location.X, e.Location.Y, 1, 1);

            //abilito/disabilito giroscopio
            if (touchRect.IntersectsWith(switchJoyStick.GetRect()))
            {
                if (sensorExists)
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

        bool isOnScreen = false;
        bool isRocketOnScreen = false;
        private void Button_Clicked(object sender, EventArgs e)            //bottombar
        {
            if (isOnScreen)
            {
                translateBottonBarDown();    //la barra non c'è più
                if (isRocketOnScreen)
                {
                    translateRocketUp();
                    isRocketOnScreen = false;
                }
                bottombartoggle.RotateXTo(0, 300);
                isOnScreen = false;
            }
            else
            {
                BottomBar.TranslateTo(0, 0, 300);      //la barra c'è
                bottombartoggle.RotateXTo(-180, 300);
                isOnScreen = true;
            }
        }

        private int rocketCount = 0;
        private Timer timer;
        private int temp;
        private void getRocketLabel(object sender, EventArgs e)                      //bottone per richiamare/far partire il razzo 
        {

            if (rocketCount == 10)
            {
                rocketCount = 0;
                Navigation.PushModalAsync(new EasterEgg());
			}

            rocketCount++;
            if (isRocketOnScreen)
             {
                 translateRocketUp();
             
                 isRocketOnScreen = false;
             }
             else
             {
                 rocketLabel.TranslateTo(0, 0, 1000, Easing.CubicOut);
                 rocketLabelImage.TranslateTo(0, 0, 1000, Easing.CubicOut);
                 isRocketOnScreen = true;
             }
        }

        private void translateBottonBarDown()
        {

            BottomBar.TranslateTo(0, BottomBar.Height - 56, 300);               

        }

        public void translateRocketUp()
		{
            rocketLabel.TranslateTo(0, -(rocketLabelImage.Height * 2), 1000 ,Easing.CubicIn);
            rocketLabelImage.TranslateTo(0, -(rocketLabelImage.Height * 2), 1000, Easing.CubicIn);

        }

        private bool theme1 = true;
        private void ChangeThemeToggle(object sender, EventArgs e)
        {
            if (theme1)
            {
                ChangeThemeButton1.FadeTo(0, 200);
                ChangeThemeButton2.FadeTo(1, 200);

                theme1 = false;
            }
            else
            {
                ChangeThemeButton2.FadeTo(0, 200);
                ChangeThemeButton1.FadeTo(1, 200);

                theme1 = true;
            }

            if (theme.Equals("image"))
                theme = "imageHD";
            else
                theme = "image";
        }

        //GIROSCOPIO
        public void updateFromSensor()  //base = se guardasse precisamente in alto  
        {
            RA = Misc.toDeg((float)main.giroscope.yaw);
            DEC = Misc.toDeg((float)main.giroscope.roll);
        }

        #endregion



        //-------------------------------------------------------------------------------------------------------------------\\
        #region FUNZIONI COORDINATE PIANETI
        //A SCHERMO
        public SKPoint toScreen(Planet body)
        {
            SKPoint cord = new SKPoint(width / 2, height / 2);

            var camDec = Misc.toRad(-DEC);
            var camRa = Misc.toRad(RA);
            var bodyDec = Misc.toRad(-body.DEC);
            var bodyRa = Misc.toRad(body.RA);
            var a = Math.Acos((Math.Sin(camDec)) * (Math.Sin(bodyDec)) + (Math.Cos(camDec)) * (Math.Cos(bodyDec)) * (Math.Cos(bodyRa - camRa)));
            var b = a / Math.Sin(a);

            var deltaX = (float)(b * (Math.Cos(bodyDec)) * (Math.Sin(bodyRa - camRa))) * width;
            var deltaY = (float)(b * ((Math.Cos(camDec)) * (Math.Sin(bodyDec)) - (Math.Sin(camDec)) * (Math.Cos(bodyDec)) * (Math.Cos(bodyRa - camRa)))) * width;
            
            if (useSensor)
            {   //decommentare per abilitare l'asse Z del telefono (funziona fino a +-90 poi è buggato)
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

        #endregion

        //-------------------------------------------------------------------------------------------------------------------\\
        #region TEXTURE

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


        private void loadBottomBarTexture()
        {
            bottombartoggle.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.uparrow.png", typeof(Camera3D).GetTypeInfo().Assembly);
            ChangeThemeButton1.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.Theme1.png", typeof(Camera3D).GetTypeInfo().Assembly);
            ChangeThemeButton2.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.Theme2.png", typeof(Camera3D).GetTypeInfo().Assembly);
            rocketLabelImage.Source = ImageSource.FromResource("Horizon.Assets.Rocket.rocketLaunch.png", typeof(Camera3D).GetTypeInfo().Assembly);
            bottombartoggle.ScaleTo(0.7);
            ChangeThemeButton1.ScaleTo(0.7);
            ChangeThemeButton2.ScaleTo(0.7);
            ChangeThemeButton2.FadeTo(0, 0);

        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------\\
        #region SELEZIONE OSSERVATORE

        private List<Planet> setObserver(List<Planet> planets, String observer)
        {
            int observerIndex = -1;                             //indice del nuovo osservatore
            float Hyp, tempx, tempy, tempz;

            this.observer = observer;

            for (int i = 0; i < planets.Count; i++)			//trovo l'indice del nuovo osservatore
                if (planets[i].name.Equals(observer))
                {
                    observerIndex = i;
                    continue;
                }
            if (observerIndex == -1)                        //se non è stato trovato l'osservatore non faccio alcuna modifica
                return planets;

            for (int i = 0; i < planets.Count; i++)			//trasformo RA e DEC in coordinate vettoriali tenendo conto della distanza dall'osservatore iniziale
            {
                planets[i].x = (float)(planets[i].distanceKm * Math.Cos(Misc.toRad(planets[i].DEC)) * Math.Cos(Misc.toRad(planets[i].RA)));
                planets[i].y = (float)(planets[i].distanceKm * Math.Cos(Misc.toRad(planets[i].DEC)) * Math.Sin(Misc.toRad(planets[i].RA)));
                planets[i].z = (float)(planets[i].distanceKm * Math.Sin(Misc.toRad(planets[i].DEC)));
            }

            tempx = planets[observerIndex].x;               //le salvo in delle variabili perchè poi vengono modificate
            tempy = planets[observerIndex].y;
            tempz = planets[observerIndex].z;
            for (int i = 0; i < planets.Count; i++)         //sottraggo (traslo) le coordinate del nuovo osservatore
            {
                planets[i].x -= tempx;
                planets[i].y -= tempy;
                planets[i].z -= tempz;
            }

            for (int i = 0; i < planets.Count; i++)         //trasformo le coordinate vettoriali in RA e DEC
            {
                Hyp = (float)Math.Sqrt(planets[i].x * planets[i].x + planets[i].y * planets[i].y);      //converto in RA e DEC
                planets[i].RA = Misc.toDeg((float)Math.Atan2(planets[i].y, planets[i].x));
                planets[i].DEC = Misc.toDeg((float)Math.Atan2(planets[i].z, Hyp));
                planets[i].distanceKm = (float)Math.Sqrt(Math.Pow(planets[i].x, 2) + Math.Pow(planets[i].y, 2) + Math.Pow(planets[i].z, 2));//aggiorno la distanza dall'osservatore così da poter ripetere l'operazione di cambio di osservatore
            }

            return planets;
        }

        private void changeObserverPressed(object sender, EventArgs e)
        {
            if (observer == "sun")
                setObserver(planets, "earth");
            else if (observer == "earth")
                setObserver(planets, "sun");
        }



		#endregion

	}
}
