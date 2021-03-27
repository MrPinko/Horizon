using System;
using System.Collections.Generic;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Horizon
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Camera2D : ContentPage
	{
		MainPage main;

		private const float popUpScale = 4;             //zoom ottimale popup
		private const float velocity = 10;                 //velocità delle animazioni zoom in/out
		private bool startup = true;

		private Point center;
		private List<Planet> pl;        //ARRAY DEI PIANETI (TERRA COMPRESA) [0]=SOLE [1]=TERRA
		private int maxDistancePx;           //DISTANZA IN PX DI RIFERMIENTO PER DISPORRE I PIANETI
		private long maxDistanceKm;          //DISTANZA MASSIMA NETTUNO
		private double height;               //ALTEZZA SCHERMO
		private double width;                //LARGHEZZA SCHERMO
		private double dpi;                  //DENSITA' SCHERMO
		private CustomButton.JoyStick joyStick;
		private CustomButton.SwitchJoyStick switchJoyStick;

		private float scale = 1, oldScale;
		private Point panPoint = new Point(0, 0);  //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info
		private bool resetCamera = false;

		private String theme;      //immagini pianeti 3 disponibili 

		private float panSens = 2f;  //la sensibilità del muoversi con un dito

		public int timeSkip = 0;            //di quanto sono traslato rispetto alla posizione iniziale
		public int skipIncrement = 0;       //di quanto incrementare/decrementare al secondo
		public bool timeIsMoving = false;   //play-pause
		private bool timeWasMoving = false;

		private List<parallaxObj> stars = new List<parallaxObj>();
		public SKPaint starPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(191, 196, 100)
		};
		public SKRect phoneRect;

		//-------------------------------------------------------------------------------------------------------------------\\
		#region COSE PRINCIPALI
		public Camera2D(MainPage main, List<Planet> pl, double height, double width, double dpi, string theme)   //COSTRUTTORE 
		{
			InitializeComponent();

			this.main = main;
			this.pl = new List<Planet>(pl);
			foreach (Planet p in this.pl)
				if (p.name == "moon")
				{
					p.dist2D = 70000000;
					p.Size = 20;
				}

			this.height = height;
			this.width = width;
			this.dpi = dpi;
			center = new Point(width / 2, height / 2);
			this.theme = theme;

			//MAXDISTANCEPX = CIRCA META' DELL'ALTEZZA DELLO SCHERMO
			maxDistancePx = ((int)height * 2);
			maxDistanceKm = 4537000000;

			//inizializzazioni
			setTexture();
			setTextureHD();
			loadBottomBarTexture();
			StarColor.initialize();

			phoneRect = SKRect.Create(0, 0, (float)width, (float)height);
			joyStick = new CustomButton.JoyStick((float)width, (float)height, (float)width / 3, (float)height / 6);
			switchJoyStick = new CustomButton.SwitchJoyStick((float)width, (float)height, 100, 100);
			setPositions();

			for (int i = 0; i < pl.Count; i++)
			{
				pl[i].sunDist = pl[i].sunDist = (float)Math.Sqrt(                   //distanza pianeta-sole
						Math.Pow(pl[i].coord.Y, 2) +
						Math.Pow(pl[i].coord.X, 2));
			}

			Random r = new Random();

			double a;
			for (int i = 0; i < 15000; i++)
			{
				if (r.NextDouble() > 0)
					a = r.NextDouble() * 0.6;
				else
					a = r.NextDouble() * 4 + 2;
				stars.Add(new parallaxObj(r.Next(-15000, 15000), r.Next(-15000, 15000), a, r.Next(50)));
			}

			sunPointer.Source = ImageSource.FromResource("Horizon.Assets.CustomButton.SunPointer.png", typeof(Camera2D).GetTypeInfo().Assembly);

		}

		//BACK
		protected override bool OnBackButtonPressed()
		{
			main.stopTimer2D = true;
			return base.OnBackButtonPressed();
		}


		//loop
		private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)             //AREA DI DISEGNO
		{
			if (startup)
			{
				translateBottonBarDown();              //abbasso al barra
				startup = false;
			}

			SKCanvas canvas = e.Surface.Canvas;
			canvas.Clear();

			if (!openPopUp)
				foreach (parallaxObj st in stars)       //stelle sotto
					if (st.paral < 1)
						canvas.DrawCircle(getPoint(st.cp.X, st.cp.Y, st.paral), (float)(3.5 * scale), StarColor.colors[st.colorIndex]);

			if (!openPopUp)
				dottedOrbit(canvas);                    //orbite

			createPlanet(canvas);                       //pianeti

			if (!openPopUp)
				foreach (parallaxObj st in stars)       //stelle sopra
					if (st.paral > 1)
						canvas.DrawCircle(getPoint(st.cp.X, st.cp.Y, st.paral), (float)(3.5 * scale), StarColor.colors[st.colorIndex]);

			//stampa puntatore sole
			updateSunPointer(canvas);

			//animazioni di cliccare ed uscire dal popup
			if (clickedPlanet)                          //pre apertura popup
				planetTranslationFunction();
			if (restoreCamera)                          //riportare lo zoom alla posizione prima dello zoom
				restoreCameraFunction();

			//creazione del popup con i suoi dati
			if (openPopUp)
				createPopUp(canvas);

			if (resetCamera)
			{
				resetCameraFunction();
			}

		}

		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region FUNZIONI DISEGNO COSE

		//creazione del popup con i suoi dati
		private void createPopUp(SKCanvas canvas)
		{
			BottomBar.IsVisible = false;
			LabelPlanetname.IsVisible = true;
			ScroolView.IsVisible = true;
			LabelPlanetname.Text = pl[iPlanet].name[0].ToString().ToUpper() + pl[iPlanet].name.Substring(1);
			drawPLanetData(canvas);
		}

		//creazione dei pianeti
		private void createPlanet(SKCanvas canvas)
		{
			if (theme.Equals("image"))           //disegno i pianeti come immagini stilizzate
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (openPopUp)
					{
						if (i == iPlanet)           //disegno solo il pianeta selezionato
							canvas.DrawBitmap(pl[i].texture, SKRect.Create((float)getPlanetPoint(pl[i]).X - (pl[i].Size * scale) / 2, (float)getPlanetPoint(pl[i]).Y - (pl[i].Size * scale) / 2, pl[i].Size * scale, pl[i].Size * scale), null);
					}
					else
						canvas.DrawBitmap(pl[i].texture, SKRect.Create((float)getPlanetPoint(pl[i]).X - (pl[i].Size * scale) / 2, (float)getPlanetPoint(pl[i]).Y - (pl[i].Size * scale) / 2, pl[i].Size * scale, pl[i].Size * scale), null);
				}
			}

			if (theme.Equals("imageHD"))            //disegno i pianeti come immagini reali
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (openPopUp)
					{
						if (i == iPlanet)
							canvas.DrawBitmap(pl[i].textureHD, SKRect.Create((float)getPlanetPoint(pl[i]).X - (pl[i].Size * scale) / 2, (float)getPlanetPoint(pl[i]).Y - (pl[i].Size * scale) / 2, pl[i].Size * scale, pl[i].Size * scale), null);
					}
					else
					{
						canvas.DrawBitmap(pl[i].textureHD, SKRect.Create((float)getPlanetPoint(pl[i]).X - (pl[i].Size * scale) / 2, (float)getPlanetPoint(pl[i]).Y - (pl[i].Size * scale) / 2, pl[i].Size * scale, pl[i].Size * scale), null);
					}
				}
			}
		}

		//aggiunta orbite con i puntini
		private void dottedOrbit(SKCanvas canvas)
		{
			for (int j = 1; j < pl.Count; j++)
			{
				if (pl[j].name == "moon")
					continue;

				float cx = (float)getPlanetPoint(pl[0]).X;        //centro cerchio x
				float cy = (float)getPlanetPoint(pl[0]).Y;           //centro cerchio y
				float r = pl[j].sunDist * scale;
				double p = r / scale * 2 * Math.PI;

				double x;
				if (j < 5)
					x = p / 22.5;    //costante grafica				//FARE UN RAPPORTO TRA LA DISTANZA DEL SOLE E '3240'
				else if (j == 5)
					x = p / 33.75;
				else
					x = p / 45;
				double a = 360 / Math.Floor(x);
				for (double i = 0; i < 360; i += a)
				{
					canvas.DrawCircle(new SKPoint((float)(cx + r * Math.Cos((i * Math.PI / 180))), (float)(cy + r * Math.Sin((i * Math.PI / 180)))), 2 * scale,       //formula per ricavare un punto sul cerchio dato l'angolo (cx+r*cos||sin(a))
						new SKPaint
						{
							Style = SKPaintStyle.Fill,
							Color = SKColors.White
						});
				}
			}
		}

		class Line
		{
			public SKPoint p1;
			public SKPoint p2;
			public Line(float x1, float y1, float x2, float y2)
			{
				p1 = new SKPoint(x1, y1);
				p2 = new SKPoint(x2, y2);
			}
		}


		private void updateSunPointer(SKCanvas canvas)
		{
			int height = (int)(canvasView.Height * dpi);
			if (!openPopUp)
				if (getPlanetPoint(pl[0]).Y < 0 || getPlanetPoint(pl[0]).Y > height || getPlanetPoint(pl[0]).X < 0 || getPlanetPoint(pl[0]).X > width)
				{
					int OSP = 70; //offset del sunPointer rispetto al bordo dello schermo, in pixel

					List<Line> sc = new List<Line>(4); //il rettangolo

					sc.Add(new Line(OSP, OSP, (float)width - OSP, OSP));
					sc.Add(new Line((float)width - OSP, OSP, (float)width - OSP, (float)height - OSP));
					sc.Add(new Line((float)width - OSP, (float)height - OSP, OSP, (float)height - OSP));
					sc.Add(new Line(OSP, (float)height - OSP, OSP, OSP));

					Line sunLine = new Line((float)(width / 2), height / 2, getPlanetPoint(pl[0]).X, getPlanetPoint(pl[0]).Y);

					SKPoint? intersection = null;
					for (int i = 0; i < sc.Count; i++)
					{
						intersection = Misc.Intersects(sc[i].p1, sc[i].p2, sunLine.p1, sunLine.p2);
						if (intersection != null)
							break;
					}

					if (intersection != null)
						sunPointer.TranslateTo(((SKPoint)intersection).X / dpi - sunPointer.X - sunPointer.Height / 2, ((SKPoint)intersection).Y / dpi - sunPointer.Y - sunPointer.Width / 2, 10);
					float b = Misc.toDeg((float)Math.Atan2(getPlanetPoint(pl[0]).Y - height / 2, getPlanetPoint(pl[0]).X - width / 2));
					//RotateTo vuole l'angolo in gradi a caso, il +90 serve perchè si e il -dim/2 perchè getPlanetPoint misura a partire dall'angolo in alto a sinistra
					sunPointer.RotateTo(b + 90, 10);
					sunPointer.IsVisible = true;
					return;
				}

			sunPointer.IsVisible = false;
		}

		private void resetCameraFunction()
		{
			d = Math.Sqrt(Math.Pow(center.X - width / 2, 2) + Math.Pow(center.Y - height / 2, 2));
			double dx = (center.X - width / 2) / d;
			double dy = (center.Y - height / 2) / d;

			double spd = 2.5;

			center.X -= dx * (d / 50 + 30);
			center.Y -= dy * (d / 50 + 30);
			Misc.println((dx * (d / 250 + 30)).ToString());
			


			if ( Math.Abs(center.X - width / 2) < 20)
				resetCamera = false;

		}
	
	

		public void drawPLanetData(SKCanvas canvas)
		{
			ScroolView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
			LeftData0.Text = pl[iPlanet].planetDataString[0].ToUpper();
			LeftData1.Text = pl[iPlanet].planetDataString[1].ToUpper();
			LeftData2.Text = pl[iPlanet].planetDataString[2].ToUpper();
			LeftData3.Text = pl[iPlanet].planetDataString[3].ToUpper();
			LeftData4.Text = pl[iPlanet].planetDataString[4].ToUpper();
			LeftData5.Text = pl[iPlanet].planetDataString[5].ToUpper();
			LeftData6.Text = pl[iPlanet].planetDataString[6].ToUpper();
			LeftData7.Text = pl[iPlanet].planetDataString[7].ToUpper();
			LeftData8.Text = pl[iPlanet].planetDataString[8].ToUpper();
			LeftData9.Text = pl[iPlanet].planetDataString[9].ToUpper();
			LeftData10.Text = pl[iPlanet].planetDataString[10].ToUpper();

			RightData0.Text = pl[iPlanet].planetData[0].ToString();
			RightData1.Text = pl[iPlanet].planetData[1].ToString();       //surface
			RightData2.Text = pl[iPlanet].planetData[2].ToString();
			RightData3.Text = pl[iPlanet].planetData[3].ToString();
			RightData4.Text = pl[iPlanet].planetData[4].ToString();
			RightData5.Text = pl[iPlanet].planetData[5].ToString();
			RightData6.Text = pl[iPlanet].planetData[6].ToString();
			RightData7.Text = pl[iPlanet].planetData[7].ToString();
			RightData8.Text = pl[iPlanet].planetData[8].ToString();
			RightData9.Text = pl[iPlanet].planetData[9].ToString();
			RightData10.Text = pl[iPlanet].planetData[10].ToString();

		}

		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region ANIMAZIONI POPUP
		private Boolean restoreCamera = false;
		public void planetTranslationFunction()
		{
			if (scale >= popUpScale)
			{
				openPopUp = true;
				clickedPlanet = false;
			}
			else if (scale < popUpScale)
			{
				scale += velocity / 30;

				center.X = -pl[iPlanet].coord.X + width / 2;
				center.Y = (-pl[iPlanet].coord.Y + height / 2) - (height / 4) / scale;
				pl[iPlanet].Size = (float)(30 * dpi);              // 80 = valore che voglio ottenere con uno schermo con un dpi di 2.6 quindi 30+2.6 e sarà ugguale ad uno schermo con 3 dpi 

			}
		}

		public void restoreCameraFunction()
		{
			BottomBar.IsVisible = true;
			if (scale <= oldScale)
				restoreCamera = false;
			else if (scale > oldScale)
				if (scale * velocity / 30 > 0)
				{
					scale -= velocity / 30;
					if (scale < oldScale)
						scale = oldScale;
				}

		}
		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region FUNZIONI TOCCO

		private Boolean clickedPlanet = false, openPopUp = false, joyStickVisible = false;
		private int iPlanet;
		private Boolean top = false, right = false, down = false, left = false;
		private int zoomGesture = 0;             //questa variabile può avere 3 stati [ 0 se non sto zommando ], [ 1 se ho finito di zoomare*], [ 2 se non sto zoommando]
												 //se ho finito di zoomare devo anche completare lo stato di movimento con variabili nulle cosi da resettare tutto 

		private void canvasView_Touch(object sender, SKTouchEventArgs e)
		{
			SKRect touchRect = SKRect.Create(e.Location.X, e.Location.Y, 1, 1);

			//listener pianeti
			if (e.ActionType == SKTouchAction.Pressed && openPopUp == false && !touchRect.IntersectsWith(joyStick.GetRect()) &&
				!touchRect.IntersectsWith(switchJoyStick.GetRect()))
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (touchRect.IntersectsWith(pl[i].hitBox(getPlanetPoint(pl[i]).X, getPlanetPoint(pl[i]).Y, pl[i].Size, scale)))
					{
						sunPointer.IsVisible = false;
						timeWasMoving = timeIsMoving;
						timeIsMoving = false;
						oldScale = scale;
						clickedPlanet = true;
						iPlanet = i;
					}
				}
			}

			//funzioni bottoni personalizzati
			if (!openPopUp)
			{
				//clicco il pulsante per mostrare il joystick
				if (touchRect.IntersectsWith(switchJoyStick.GetRect()))
				{
					joyStickVisible = !joyStickVisible;
					switchJoyStick.changeStateOn();
				}

				//clicco il joystick
				if (e.ActionType == SKTouchAction.Pressed && joyStickVisible)
				{
					if (touchRect.IntersectsWith(joyStick.getTop()))          //tasto sopra
					{
						top = true;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getDown()))           //sotto
					{
						down = true;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getRight()))         //destra
					{
						right = true;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getleft()))        //sinsitra
					{
						left = true;
						e.Handled = true;
					}
				}
			}

			//aperto il popup se clicco fuori da essi esco dalla modalità
			if (openPopUp && touchRect.IntersectsWith(phoneRect))             //non prende il tocco se clicco un elemento xaml
			{
				openPopUp = false;
				LabelPlanetname.IsVisible = false;
				ScroolView.IsVisible = false;
				timeIsMoving = timeWasMoving;

				restoreCamera = true;
				if (iPlanet == 2)      //luna
					pl[iPlanet].Size = 20;
				else
					pl[iPlanet].Size = 70;         //ripristino le dimensioni del pianeta quando esco dalla visualizzazione popup
			}

			//rilascio la pressione su un tasto del joystick
			if (e.ActionType == SKTouchAction.Released)
			{
				top = false;
				right = false;
				down = false;
				left = false;
				e.Handled = false;
			}
		}

		private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)  //dito che preme e si muove in giro
		{
			if (!clickedPlanet && !openPopUp && !joyStickVisible && zoomGesture != 2)
			{
				if (e.StatusType.ToString() == "Running" && zoomGesture != 1)
				{
					panPoint.X = e.TotalX * panSens / scale;  //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info
					panPoint.Y = e.TotalY * panSens / scale;
				}
				else if (e.StatusType.ToString() == "Completed")  //quando il dito finisce di spostarsi il centro della telecamera si muove
				{
					zoomGesture = 0;
					center.X += panPoint.X;
					center.Y += panPoint.Y;
					panPoint.X = 0;
					panPoint.Y = 0;
				}
			}
		}

		private void PinchGestureRecognizer_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e) //due dita che zoommano
		{
			if (!openPopUp)
			{
				if (e.Status.ToString() == "Running")
				{
					if (scale * (float)e.Scale <= 4)
						scale *= (float)e.Scale;
					zoomGesture = 2;
				}
				else if (e.Status.ToString() == "Completed")
				{
					zoomGesture = 1;
				}


			}
		}

		public void joyStickMovementListener()
		{
			if (top)
				center.Y -= (2f / scale);
			if (right)
				center.X += 2f / scale;
			if (down)
				center.Y += 2f / scale;
			if (left)
				center.X -= 2f / scale;
		}

		bool isOnScreen = false;
		private void Button_Clicked(object sender, EventArgs e)
		{
			if (isOnScreen)
			{
				translateBottonBarDown();    //la barra non c'è più
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

		private void translateBottonBarDown()
		{

			BottomBar.TranslateTo(0, BottomBar.Height - 56, 300);               //-56 = -46 - 10

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
		double d;
		private void sunPointer_gesture(object sender, EventArgs e)
		{
			resetCamera = true;
		}

		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region TIME SKIP
		public void timeChanged()
		{
			for (int i = 1; i < pl.Count; i++)
			{
				double a = Math.Atan2(pl[i].originalCoord.Y, pl[i].originalCoord.X);        //angolo originario tra sole-pianeta
				a += pl[i].orbitRateo * Misc.toRad(timeSkip);         //ad ogni tick la terra si muove di 45 gradi, modificare il 45 per aumentare/diminuire la rotazione
				a = a % (2 * Math.PI);
				if (a < 0)
					a += 2 * Math.PI;

				pl[i].coord.X = (int)(Math.Cos(a) * pl[i].sunDist);                     //aggiorno cord
				pl[i].coord.Y = (int)(Math.Sin(a) * pl[i].sunDist);
			}
		}
		private void backPressed(object sender, EventArgs e)
		{
			timeIsMoving = true;

			if (skipIncrement > 0 && skipIncrement < 32)
				skipIncrement *= 2;
			else if (skipIncrement == 0)
				skipIncrement = 2;
			else if (skipIncrement == -2)
				skipIncrement = 0;
			else if (skipIncrement < -2)
				skipIncrement /= 2;

		}

		private void forwardPressed(object sender, EventArgs e)
		{
			timeIsMoving = true;

			if (skipIncrement > 2)
				skipIncrement /= 2;
			else if (skipIncrement == 2)
				skipIncrement = 0;
			else if (skipIncrement == 0)
				skipIncrement = -2;
			else if (skipIncrement < 0 && skipIncrement > -32)
				skipIncrement *= 2;

		}

		private void stopPressed(object sender, EventArgs e)
		{
			timeIsMoving = false;
			skipIncrement = 0;
		}

		private void resetPressed(object sender, EventArgs e)
		{
			timeIsMoving = false;
			skipIncrement = 0;
			timeSkip = 0;
			timeChanged();
		}

		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region FUNZIONI COORDINATE PIANETI

		private void setPositions()           //SETTO LE COORDINATE DI TUTTI I PIANETI
		{
			//SETTO IL SOLE
			pl[0].coord.X = 0;
			pl[0].coord.Y = 0;

			//SETTO LA TERRA
			pl[1].coord = setPlanet((int)pl[0].coord.X, (int)pl[0].coord.Y, (pl[0].RA + 180) % 360, pl[0].dist2D);
			pl[1].originalCoord = setPlanet((int)pl[0].coord.X, (int)pl[0].coord.Y, (pl[0].RA + 180) % 360, pl[0].dist2D);

			//SETTO I PIANETI RIMANENTI
			for (int i = 2; i < pl.Count; i++)
			{
				pl[i].coord = setPlanet((int)pl[1].coord.X, (int)pl[1].coord.Y, pl[i].RA, pl[i].dist2D);
				pl[i].originalCoord = setPlanet((int)pl[1].coord.X, (int)pl[1].coord.Y, pl[i].RA, pl[i].dist2D);
			}
		}

		private Point setPlanet(int x, int y, float RA, float dis2D)   //SETTO UN PIANETA
		{
			float disPx = maxDistancePx * dis2D / maxDistanceKm;
			int deltaX = (int)(Math.Cos((Math.PI / 180) * RA) * disPx);
			int deltaY = (int)(Math.Sin((Math.PI / 180) * RA) * disPx);
			return new Point(x + deltaX, y + deltaY);
		}

		private SKPoint getPlanetPoint(Planet pl)
		{
			return getPoint(pl.coord.X, pl.coord.Y, 1);
		}

		private SKPoint getPoint(double x, double y, double paral)
		{
			return new SKPoint(
				(float)(x * scale +                                     //la posizione iniziale del pianeta (che non viene mai cambiata!) zoommata  +
				width / 2 + (center.X - width / 2) * scale * paral +    //il centro della telecamera zoommato (with / 2 non deve venir scalato però quindi lo tiro fuori)  +
				panPoint.X * scale * paral),                            //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info

				(float)(y * scale +                                     //stessa cosa ma per la Y
				height / 2 + (center.Y - height / 2) * scale * paral +
				panPoint.Y * scale * paral));
		}


		public void loop()
		{
			canvasView.InvalidateSurface();
		}

		private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{

		}

		#endregion

		//-------------------------------------------------------------------------------------------------------------------\\
		#region TEXTURE

		private List<String> texturepath = new List<string>();

		private List<String> texturepathHD = new List<string>();

		private void setTexture()
		{
			loadTexture();
			for (int i = 0; i < texturepath.Count; i++)
			{
				pl[i].setTexture(texturepath[i]);
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
				pl[i].setTextureHD(texturepathHD[i]);
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
			bottombartoggle.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.uparrow.png", typeof(Camera2D).GetTypeInfo().Assembly);
			backBtn.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.backarrow.png", typeof(Camera2D).GetTypeInfo().Assembly);
			stopBtn.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.pause.png", typeof(Camera2D).GetTypeInfo().Assembly);
			resetBtn.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.hourglass.png", typeof(Camera2D).GetTypeInfo().Assembly);
			forwardBtn.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.notbackarrow.png", typeof(Camera2D).GetTypeInfo().Assembly);
			ChangeThemeButton1.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.Theme1.png", typeof(Camera2D).GetTypeInfo().Assembly);
			ChangeThemeButton2.Source = ImageSource.FromResource("Horizon.Assets.BottomBar.Theme2.png", typeof(Camera2D).GetTypeInfo().Assembly);
			bottombartoggle.ScaleTo(0.7);
			backBtn.ScaleTo(0.7);
			stopBtn.ScaleTo(0.7);
			resetBtn.ScaleTo(0.7);
			forwardBtn.ScaleTo(0.7);
			ChangeThemeButton1.ScaleTo(0.7);
			ChangeThemeButton2.ScaleTo(0.7);
			ChangeThemeButton2.FadeTo(0, 0);


		}

		#endregion

	}
}