﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp;
using System.Reflection;
using Xamarin.Essentials;
using Android.App;
using Android.OS;
using System.Threading;
using Android.Views.InputMethods;
using Android.Content;

namespace Horizon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Camera3D : ContentPage
    {
        private bool startup = true;
        private double dpi = DeviceDisplay.MainDisplayInfo.Density;

        private MainPage main;
        private List<Planet> planets;           //lista dei pianeti guardando dal polo nord, usata nella conversione quando si cambia osservatore e come punto di partenza quando si cambia la posizione GPS
        private List<Planet> syncPlanets;       //lista dei pianeti dal punto di vista della terra, sincronizzata con la posizione
        private List<Planet> tempPlanets;       //lista dei pianeti temporanea
        private List<Planet> stars;
        private List<Planet> syncStars;
        private List<Planet> tempStars;
        private Constellations constellations;
        private Constellations syncConstellations;
        private Constellations tempConstellations;
        private List<Planet> points;
        private List<Planet> syncPoints;
        private List<Planet> tempPoints;
        private String observer;

        private String theme;
        private List<String> texturepath = new List<string>();
        private List<String> texturepathHD = new List<string>();
        public Boolean useSensor = true;    //lasciare public
        public Boolean sensorExists = true; //same here
        private Boolean positionLoaded = true;
        private Boolean usingGPS = true;
        private Boolean isCustomLocationOpen = false;

        private const float baseAmp = 72 / 2; //72 e 90 best imho
        private double RA;      //in gradi
        private double DEC;     //in gradi
        private int width;
        private int height;
        private float ampWidth;
        private float ampHeight;

        private Point panPoint = new Point(0, 0);
        private float latitude;
        private float longitude;
        private float customLatitude;
        private float customLongitude;
        private float tempDEC;
        private float tempRA;
        public float baseX;
        public float baseY;

        //paint per i punti di riferimento (nord, sud, cerchio nell'equatore)
        public SKPaint uselessPaint = new SKPaint{
            Style = SKPaintStyle.Fill,
            Color = new SKColor(224, 224, 224)};

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
        public Camera3D(MainPage main, List<Planet> planets, String observer, float RA, float DEC, float longitude, Boolean isLocationLoaded, int height, int width, String theme)    //quel dec è dove guardiamo se guardiamo in alto
        {
            InitializeComponent();
            this.main = main;
            this.planets = new List<Planet>(planets);
            stars = StarDB.getAll();
            points = new List<Planet>(planets);
            points.Add(new Planet("SOUTHPOLE", 0, -90, 60, new SKColor(255, 255, 255)));
            points.Add(new Planet("NORTHPOLE", 0, 90, 60, new SKColor(255, 255, 255)));
            for (float i = 0; i <= 360; i = i + 0.2f)
                points.Add(new Planet("EQUATOR", i, 0, 4, new SKColor(255, 255, 255)));

            this.observer = observer;
            this.RA = tempRA = RA;
            this.DEC = tempDEC = latitude = customLatitude = DEC;
            this.longitude = customLongitude = longitude;
            positionLoaded = usingGPS = isLocationLoaded;
            this.width = width;
            this.height = height;
            ampWidth = baseAmp / 2;
            ampHeight = ampWidth * height / width;
            this.theme = theme;

            //sync
            this.syncPlanets = new List<Planet>();
            for (int i = 0; i < planets.Count; i++)
                syncPlanets.Add((Planet)planets[i].Clone());
            syncPlanets = synchronizePlanets(syncPlanets, customLongitude, customLatitude);

            this.syncStars = new List<Planet>();
            for (int i = 0; i < stars.Count; i++)
                syncStars.Add((Planet)stars[i].Clone());
            syncStars = synchronizePlanets(syncStars, customLongitude, customLatitude);

            this.syncPoints = new List<Planet>();
            for (int i = 0; i < points.Count; i++)
                syncPoints.Add((Planet)points[i].Clone());
            syncPoints = synchronizePlanets(syncPoints, customLongitude, customLatitude);

            //costellazioni
            this.constellations = new Constellations(this, stars);
            this.constellations.printText = true;
            this.syncConstellations = new Constellations(this, syncStars);
            this.syncConstellations.printText = true;

            //fix polo nord e sud
            FixNorthSouthPole();

            //inizializzazioni
            setTexture();
            setTextureHD();
            loadBottomBarTexture();

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

            //seleziono le liste sincronizzate (terra) o non sincronizzate (tutto il resto)
            if (observer.Equals("earth"))
            {
                tempPlanets = syncPlanets;
                tempConstellations = syncConstellations;
                tempStars = syncStars;
                tempPoints = syncPoints;
            }
            else
            {
                tempPlanets = planets;
                tempConstellations = constellations;
                tempStars = stars;
                tempPoints = points;
            }

            //stampo le stelle
            for (int i = 0; i < tempStars.Count; i++)
                canvas.DrawCircle(toScreen(tempStars[i]), 1, tempStars[i].paint);

            //stampo le costellazioni
            //SI BUGGA SE METTEREMO LO ZOOM, CHIEDI AL PIETRO
            tempConstellations.drawAll(canvas);

            //stampo i punti di riferimento (sud, nord, equatore)
            for (int i = 0; i < tempPoints.Count; i++)
            {
                if(tempPoints[i].name.Equals("NORTHPOLE"))
                    canvas.DrawCircle(toScreen(tempPoints[i]), 60, uselessPaint);
                else if(tempPoints[i].name.Equals("SOUTHPOLE"))
                    canvas.DrawCircle(toScreen(tempPoints[i]), 60, uselessPaint);
                else
                    canvas.DrawCircle(toScreen(tempPoints[i]), 4, uselessPaint);
            }
            /*canvas.DrawCircle(toScreen(new Planet("SOUTHPOLE", 0, -90, 10, new SKColor(255, 255, 255))), 60, uselessPaint);
            canvas.DrawCircle(toScreen(new Planet("NORTHPOLE", 0, 90, 10, new SKColor(127, 127, 127))), 60, uselessPaint);
            for (float i = 0; i <= 360; i = i + 0.2f)
                canvas.DrawCircle(toScreen(new Planet("EQUATOR", i, 0, 3, new SKColor(0, 127, 127))), 5, uselessPaint);*/

            //stampo i pianeti
            for (int i = 0; i < tempPlanets.Count; i++)
            {
                if (tempPlanets[i].name == observer)  //non stampo la terra o il sole in base dal punto di vista
                    continue;

                SKPoint tempPoint = toScreen(tempPlanets[i]);
                //sposto le coordinate di stampa del pianeta in base alla dimensione con cui viene stampato (drawbitmap non disegna partendo dal centro)
                tempPoint.X -= (200 + tempPlanets[i].printSize * 15) / 2;
                tempPoint.Y -= (200 + tempPlanets[i].printSize * 15) / 2;

                if (theme.Equals("image"))              //disegno i pianeti come immagini stilizzate
                    canvas.DrawBitmap(tempPlanets[i].texture, SKRect.Create(tempPoint, new SKSize(200 + tempPlanets[i].printSize * 15, 200 + tempPlanets[i].printSize * 15)), null);
                else if (theme.Equals("imageHD"))            //disegno i pianeti come immagini reali
                    canvas.DrawBitmap(tempPlanets[i].textureHD, SKRect.Create(tempPoint, new SKSize(200 + tempPlanets[i].printSize * 15, 200 + tempPlanets[i].printSize * 15)), null);
            }
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
        private void GPSModePressed(object sender, EventArgs e)  //apro la personalizzazione della posizione
        {
            if (!observer.Equals("earth"))                       //disponibile solo con la terra
                return;

            if (isCustomLocationOpen)
            {
                CloseCustomGPS();
                return;
            }

            if (!isBarOnScreen)
                return;

            if (isRocketOnScreen)
            {
                translateRocketUp();
                isRocketOnScreen = false;
            }

            if (!positionLoaded)
            {
                normalGPSLabel.IsEnabled = false;
                normalGPSRadio.IsEnabled = false;
                customGPSRadio.IsChecked = true;
            }
            else if (usingGPS)
            {
                normalGPSRadio.IsChecked = true;
            }
            else if (!usingGPS)
            {
                customGPSRadio.IsChecked = true;
            }
            latitudeText.Text = customLatitude.ToString();
            longitudeText.Text = customLongitude.ToString();

            GPSBackground.IsVisible = true;
            
            normalGPSRadio.IsVisible = true;
            customGPSRadio.IsVisible = true;

            normalGPSLabel.IsVisible = true;
            customGPSLabel.IsVisible = true;

            latitudeLabel.IsVisible = true;
            longitudeLabel.IsVisible = true;
            latitudeText.IsVisible = true;
            longitudeText.IsVisible = true;

            cancelLabel.IsVisible = true;
            applyLabel.IsVisible = true;

            GPSBackground.FadeTo(1, 200);

            normalGPSRadio.FadeTo(1, 200);
            customGPSRadio.FadeTo(1, 200);

            normalGPSLabel.FadeTo(1, 200);
            customGPSLabel.FadeTo(1, 200);

            latitudeLabel.FadeTo(1, 200);
            longitudeLabel.FadeTo(1, 200);
            latitudeText.FadeTo(1, 200);
            longitudeText.FadeTo(1, 200);

            cancelLabel.FadeTo(1, 200);
            applyLabel.FadeTo(1, 200);

            isCustomLocationOpen = true;
        }

        private void NormalGPSPressed(object sender, EventArgs e)   //listener per la label del radio
        {
            normalGPSRadio.IsChecked = true;
        }

        private void CustomGPSPressed(object sender, EventArgs e)   //listener per la label del radio
        {
            customGPSRadio.IsChecked = true;
        }

        private void ApplyPressed(object sender, EventArgs e)  //chiudo la personalizzazione e applico le modifiche
        {
            if (normalGPSRadio.IsChecked)
            {
                usingGPS = true;
                customLatitude = latitude;
                customLongitude = longitude;
                GPSMode1.FadeTo(1, 200);
                GPSMode2.FadeTo(0, 200);
            }
            else
            {
                usingGPS = false;
                customLatitude =  float.Parse(latitudeText.Text.Replace('.', ','));
                customLongitude = float.Parse(longitudeText.Text.Replace('.', ','));
                if (customLatitude > 90)
                    customLatitude = 90;
                else if (customLatitude < -90)
                    customLatitude = -90;
                if (customLongitude > 360)
                    customLongitude = 360;
                else if (customLongitude < 0)
                    customLongitude = 0;
                GPSMode1.FadeTo(0, 200);
                GPSMode2.FadeTo(1, 200);
            }

            for(int i = 0; i < planets.Count; i++)
                syncPlanets[i] = (Planet)planets[i].Clone();
            for (int i = 0; i < stars.Count; i++)
                syncStars[i] = (Planet)stars[i].Clone();
            for (int i = 0; i < points.Count; i++)
                syncPoints[i] = (Planet)points[i].Clone();

            syncPlanets = synchronizePlanets(syncPlanets, customLongitude, customLatitude);
            syncStars = synchronizePlanets(syncStars, customLongitude, customLatitude);
            syncPoints = synchronizePlanets(syncPoints, customLongitude, customLatitude);
            syncConstellations = new Constellations(this, syncStars);
            FixNorthSouthPole();

            CloseCustomGPS();
        }

        private void CancelPressed(object sender, EventArgs e)  //chiudo la personalizzazione senza applicare le modifiche
        {
            CloseCustomGPS();
        }

        public class UiThread : Activity        //serve per creare un thread che ritardi il isVisible = false del menu del GPS(thread sucks lol)
        {                                       //e per chiudere la tastiera quando perde focus
            Camera3D cam3D;

            public UiThread(Camera3D cam3D)
            {
                this.cam3D = cam3D;
            }

            protected override void OnCreate(Bundle bundle)
            {
                base.OnCreate(bundle);
                ThreadPool.QueueUserWorkItem(o => SlowMethod());
            }

            private void SlowMethod()
            {
                Thread.Sleep(200);
                RunOnUiThread(() => {
                    cam3D.GPSBackground.IsVisible = false;

                    cam3D.normalGPSRadio.IsVisible = false;
                    cam3D.customGPSRadio.IsVisible = false;

                    cam3D.normalGPSLabel.IsVisible = false;
                    cam3D.customGPSLabel.IsVisible = false;

                    cam3D.latitudeLabel.IsVisible = false;
                    cam3D.longitudeLabel.IsVisible = false;
                    cam3D.latitudeText.IsVisible = false;
                    cam3D.longitudeText.IsVisible = false;

                    cam3D.cancelLabel.IsVisible = false;
                    cam3D.applyLabel.IsVisible = false; });
            }
        }

        private void CloseCustomGPS() //chiudo la personalizzazione della posizione
        {
            GPSBackground.FadeTo(0, 200);       //sfocatura in uscita

            normalGPSRadio.FadeTo(0, 200);
            customGPSRadio.FadeTo(0, 200);

            normalGPSLabel.FadeTo(0, 200);
            customGPSLabel.FadeTo(0, 200);

            latitudeLabel.FadeTo(0, 200);
            longitudeLabel.FadeTo(0, 200);
            latitudeText.FadeTo(0, 200);
            longitudeText.FadeTo(0, 200);

            cancelLabel.FadeTo(0, 200);
            applyLabel.FadeTo(0, 200);

            UiThread uselessThread = new UiThread(this);    //thread che ritarda il setVisible = false e chiude la tastiera

            var currentFocus = uselessThread.CurrentFocus;
            if (currentFocus != null)
            {
                InputMethodManager inputMethodManager = (InputMethodManager)uselessThread.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }

            isCustomLocationOpen = false;
        }

        private void SwitchJoystickToggle(object sender, EventArgs e)
        {
            if (!isBarOnScreen)
                return;

            if (sensorExists)
            {
                useSensor = !useSensor;
                if (!useSensor)
                {
                    tempRA = (float)RA;
                    tempDEC = (float)DEC;
                    SwitchJoystickButton1.FadeTo(0, 200);
                    SwitchJoystickButton2.FadeTo(1, 200);
                }
                else
                {
                    SwitchJoystickButton1.FadeTo(1, 200);
                    SwitchJoystickButton2.FadeTo(0, 200);
                }
            }
        }

        bool isBarOnScreen = false;
        bool isRocketOnScreen = false;
        private void Button_Clicked(object sender, EventArgs e)            //bottombar
        {
            if (isBarOnScreen)
            {
                translateBottonBarDown();    //la barra non c'è più
                if (isCustomLocationOpen)
                {
                    CloseCustomGPS();
                }
                if (isRocketOnScreen)
                {
                    translateRocketUp();
                    isRocketOnScreen = false;
                }
                bottombartoggle.RotateXTo(0, 300);
                isBarOnScreen = false;
            }
            else
            {
                BottomBar.TranslateTo(0, 0, 300);      //la barra c'è
                bottombartoggle.RotateXTo(-180, 300);
                isBarOnScreen = true;
            }
        }

        private int rocketCount = 0;
        private Timer timer;
        private int temp;
        private void getRocketLabel(object sender, EventArgs e)                      //bottone per richiamare/far partire il razzo 
        {
            if (!isBarOnScreen)
                return;

            if (rocketCount == 10)
            {
                rocketCount = 0;
                if (isCustomLocationOpen)
                    CloseCustomGPS();
                Navigation.PushModalAsync(new EasterEgg());
                return;
			}

            rocketCount++;
            if (isRocketOnScreen)
            {
                 translateRocketUp();
                 isRocketOnScreen = false;
            }
            else
            {
                if(isCustomLocationOpen)
                    CloseCustomGPS();
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
            isRocketOnScreen = false;
        }

        private bool theme1 = true;
        private void ChangeThemeToggle(object sender, EventArgs e)
        {
            if (!isBarOnScreen)
                return;

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

        //GIROSCOPIO
        public void updateFromSensor()  //base = se guardasse precisamente in alto  
        {
            RA = Misc.toDeg((float)main.giroscope.yaw);
            DEC = Misc.toDeg((float)main.giroscope.roll);
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
                syncPlanets[i].setTexture(texturepath[i]);
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
                syncPlanets[i].setTextureHD(texturepathHD[i]);
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
            SwitchJoystickButton1.Source = ImageSource.FromResource("Horizon.Assets.CustomButton.controllerSwitch.png", typeof(Camera3D).GetTypeInfo().Assembly);
            SwitchJoystickButton2.Source = ImageSource.FromResource("Horizon.Assets.CustomButton.controllerSwitch2.png", typeof(Camera3D).GetTypeInfo().Assembly);
            GPSMode1.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.gpsEnabled.png", typeof(Camera3D).GetTypeInfo().Assembly);
            GPSMode2.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.gpsDisabled.png", typeof(Camera3D).GetTypeInfo().Assembly);
            rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.earth.png", typeof(Camera3D).GetTypeInfo().Assembly);

            bottombartoggle.ScaleTo(0.7);
            ChangeThemeButton1.ScaleTo(0.7);
            ChangeThemeButton2.ScaleTo(0.7);
            SwitchJoystickButton1.ScaleTo(0.7);
            SwitchJoystickButton2.ScaleTo(0.7);
            GPSMode1.ScaleTo(0.7);
            GPSMode2.ScaleTo(0.7);

            normalGPSRadio.FadeTo(0, 0);
            customGPSRadio.FadeTo(0, 0);

            normalGPSLabel.FadeTo(0, 0);
            customGPSLabel.FadeTo(0, 0);

            latitudeLabel.FadeTo(0, 0);
            longitudeLabel.FadeTo(0, 0);
            latitudeText.FadeTo(0, 0);
            longitudeText.FadeTo(0, 0);

            cancelLabel.FadeTo(0, 0);
            applyLabel.FadeTo(0, 0);

            ChangeThemeButton2.FadeTo(0, 0);
            if(positionLoaded)
                GPSMode2.FadeTo(0, 0);
            else
                GPSMode1.FadeTo(0, 0);
            if (sensorExists)
                SwitchJoystickButton2.FadeTo(0, 0);
            else
                SwitchJoystickButton1.FadeTo(0, 0);
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------\\
        #region SELEZIONE OSSERVATORE

        private List<Planet> setObserver(List<Planet> planets, String observer)
        {
            int observerIndex = -1;                         //indice del nuovo osservatore
            float Hyp, tempx, tempy, tempz;

            for (int i = 0; i < planets.Count; i++)			//trovo l'indice del nuovo osservatore
                if (planets[i].name.Equals(observer))
                {
                    observerIndex = i;
                    continue;
                }
            if (observerIndex == -1)                        //se non è stato trovato l'osservatore non faccio alcuna modifica
                return planets;

            this.observer = observer;                       //aggiorno l'osservatore

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
                planets[i].distanceKm = (float)Math.Sqrt(Math.Pow(planets[i].x, 2) + Math.Pow(planets[i].y, 2) + Math.Pow(planets[i].z, 2));    //aggiorno la distanza dall'osservatore così da poter ripetere l'operazione di cambio di osservatore
            }

            return planets;
        }

        private void changeObserverPressed(object sender, EventArgs e)
        {
            if (sender.Equals(sunLabel))
            {
                setObserver(planets, "sun");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.sun.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(mercuryLabel))
            {
                setObserver(planets, "mercury");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.mercury.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(venusLabel))
            {
                setObserver(planets, "venus");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.venus.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(earthLabel))
            {
                setObserver(planets, "earth");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.earth.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(moonLabel))
            {
                setObserver(planets, "moon");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.moon.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(marsLabel))
            {
                setObserver(planets, "mars");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.mars.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(jupiterLabel))
            {
                setObserver(planets, "jupiter");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.jupiter.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(saturnLabel))
            {
                setObserver(planets, "saturn");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.saturn.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(neptuneLabel))
            {
                setObserver(planets, "neptune");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.neptune.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
            else if (sender.Equals(uranusLabel))
            {
                setObserver(planets, "uranus");
                rocketButton.ImageSource = ImageSource.FromResource("Horizon.Assets.BottomBar.uranus.png", typeof(Camera3D).GetTypeInfo().Assembly);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------\\
        #region SYNC

        private List<Planet> synchronizePlanets(List<Planet> planets, float newLongitude, float newLatitude)   //conversione sistema di riferimento posizione utente
        {
            //https://it.wikipedia.org/wiki/Coordinate_celesti#Conversione_tra_coordinate_di_diversi_sistemi_di_riferimento for more info

            float H, radLatitude, radDEC, newDEC, newRA;
            radLatitude = Misc.toRad(newLatitude);
            for (int i = 0; i < planets.Count; i++)
            {
                H = Misc.toRad((float)sidTime.getSiderealTimeFromLongitude(newLongitude) - planets[i].RA);
                radDEC = Misc.toRad(planets[i].DEC);

                newDEC = (float)Math.Asin(Math.Sin(radDEC) * Math.Sin(radLatitude) + Math.Cos(radDEC) * Math.Cos(radLatitude) * Math.Cos(H));
                newRA = (float)Math.Atan2(Math.Sin(H), Math.Tan(radDEC) * Math.Cos(radLatitude) - Math.Sin(radLatitude) * Math.Cos(H));

                planets[i].DEC = Misc.toDeg(newDEC);
                planets[i].RA = 360 - Misc.toDeg(newRA);
            }

            return planets;
        }

        //FIX AL RIFERIMENTO DEL POLO SUD E NORD CHE A VOLTE SI BUGGANO
        private void FixNorthSouthPole()
        {
            float ursaRA = -420, ursaDEC = -420, ursaX, ursaY;
            float poleRA = -420, poleDEC = -420, poleX, poleY;

            //coordinate stella polare
            for (int i = 0; i < syncConstellations.cons.Length; i++)
                if (syncConstellations.cons[i].code.Equals("Ursa Minor"))
                {
                    ursaRA = syncConstellations.cons[i].costName.RA;
                    ursaDEC = syncConstellations.cons[i].costName.DEC;
                    break;
                }
            if (ursaRA == -420 && ursaDEC == -420)
            {
                Misc.println("\n\n ORSA MINORE NON TROVATA \n\n");
                return;
            }
            ursaX = (float)(Math.Cos(Misc.toRad(ursaDEC)) * Math.Cos(Misc.toRad(ursaRA)));
            ursaY = (float)(Math.Cos(Misc.toRad(ursaDEC)) * Math.Sin(Misc.toRad(ursaRA)));

            //coordinate polo nord
            for (int i = 0; i < syncPoints.Count; i++)
                if (syncPoints[i].name.Equals("NORTHPOLE"))
                {
                    poleRA = syncPoints[i].RA;
                    poleDEC = syncPoints[i].DEC;
                    break;
                }
            if (poleRA == -420 && poleDEC == -420)
            {
                Misc.println("\n\n POLO NORD NON TROVATO \n\n");
                return;
            }
            poleX = (float)(Math.Cos(Misc.toRad(poleDEC)) * Math.Cos(Misc.toRad(poleRA)));
            poleY = (float)(Math.Cos(Misc.toRad(poleDEC)) * Math.Sin(Misc.toRad(poleRA)));

            //calcolo il delta e in caso fixo
            if (Math.Abs(ursaX - poleX) > 0.2 || Math.Abs(ursaY - poleY) > 0.2)     //le X e le Y vanno da -1 a 1, lo 0.2 è a caso ma funziona
                for (int i = 0; i < syncPoints.Count; i++)
                    if (syncPoints[i].name.Equals("SOUTHPOLE") || syncPoints[i].name.Equals("NORTHPOLE"))
                    {
                        syncPoints[i].RA += 180;
                        if (syncPoints[i].RA > 360)
                            syncPoints[i].RA -= 360;
                    }
        }

        #endregion
    }
}
