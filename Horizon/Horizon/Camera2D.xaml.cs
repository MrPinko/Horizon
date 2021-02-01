using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Horizon
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Camera2D : ContentPage
	{
		MainPage main;

		private const float popUpScale = 4;             //zoom ottimale popup
		private const float velocity = 10;                 //velocità delle animazioni zoon in/out
		private double dpi = DeviceDisplay.MainDisplayInfo.Density;

		private Point center;
		private List<Planet> pl;        //ARRAY DEI PIANETI (TERRA COMPRESA) [0]=SOLE [1]=TERRA
		private int maxDistancePx;           //DISTANZA IN PX DI RIFERMIENTO PER DISPORRE I PIANETI
		private long maxDistanceKm;          //DISTANZA MASSIMA NETTUNO
		private double height;               //ALTEZZA SCHERMO
		private double width;                //LARGHEZZA SCHERMO
		private CustomButton.ChangeTextureButton changeButton;
		private CustomButton.JoyStick joyStick;
		private CustomButton.SwitchJoyStick switchJoyStick;

		private float scale = 1, oldScale;
		private Point panPoint = new Point(0, 0);  //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info

		private String theme;      //immagini pianeti 3 disponibili

		private float panSens = 2f;  //la sensibilità del muoversi con un dito

		public Camera2D(MainPage main,List<Planet> pl, double height, double width, string theme)   //COSTRUTTORE SERIO
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
			center = new Point(width / 2, height / 2);
			this.theme = theme;

			//MAXDISTANCEPX = CIRCA META' DELL'ALTEZZA DELLO SCHERMO
			maxDistancePx = ((int)height * 2);
			maxDistanceKm = 4537000000;

			setTexture();
			setTextureHD();

			changeButton = new CustomButton.ChangeTextureButton((float)width, (float)height, 150, 150);
			joyStick = new CustomButton.JoyStick((float)width, (float)height, (float)width / 3, (float)height / 6);
			switchJoyStick = new CustomButton.SwitchJoyStick((float)width, (float)height, 100, 100);
			setPositions();
		}

		//BACK
		protected override bool OnBackButtonPressed()
		{
			main.stopTimer2D = true;
			return base.OnBackButtonPressed();
		}
		
		//------------------------------------------------------------------------------------------------------------------\\
		//FUNZIONI DISEGNO
		public List<Point> test = new List<Point>();

		//disegno
		private SKRect topPopUp, downPopUp;

		private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;
			canvas.Clear();

			//creazione orbite
			if(!openPopUp)
				dottedOrbit(canvas);

			//creazione dei pianeti
			createPlanet(canvas);

			//visibilità joystick
			if (!openPopUp)
			{
				changeButton.draw(canvas);
				if (joyStickVisible)
					joyStick.draw(canvas);
				switchJoyStick.draw(canvas);
			}

			//movimento con il joystick
			if (top)
				center.Y -= (2f / scale);

			if (right)
				center.X += 2f / scale;

			if (down)
				center.Y += 2f / scale;

			if (left)
				center.X -= 2f / scale;

			if (clickedPlanet)                      //pre apertura popup
			{
				if (scale >= popUpScale)
				{
					openPopUp = true;
					clickedPlanet = false;
				}
				else if (scale < popUpScale)
				{
					scale += velocity / 30;

					center.X = -pl[iPlanet].coord.X + width / 2 - pl[iPlanet].Size / 2;
					center.Y = (-pl[iPlanet].coord.Y + height / 2 - pl[iPlanet].Size / 2) - (height / 4) / scale;
					pl[iPlanet].Size =(float) (30 * dpi);              // 80 = valore che voglio ottenere con uno schermo con un dpi di 2.6 quindi 30+2.6 e sarà ugguale ad uno schermo con 3 dpi 
					
				}

			}

			if (restoreCamera)
			{
				if (scale <= oldScale)
					restoreCamera = false;
				else
					if (scale > oldScale)
					if (scale * velocity / 30 > 0)
						scale -= velocity / 30;
			}

			//creazione del popup con i suoi dati
			if(openPopUp)
				createPopUp(canvas);
		}

		//creazione del popup con i suoi dati
		private void createPopUp(SKCanvas canvas)
		{
			LabelPlanetname.IsVisible = true;
			LabelPlanetname.Text = pl[iPlanet].name[0].ToString().ToUpper() + pl[iPlanet].name.Substring(1);
			drawPLanetData(canvas);
		}

		//creazione dei pianeti
		private void createPlanet(SKCanvas canvas)
		{
			if (theme.Equals("circle"))            //non implementata see soon
			{
				for (int i = 0; i < pl.Count; i++)
				{
					canvas.DrawCircle((float)getPlanetPoint(pl[i]).X, (float)getPlanetPoint(pl[i]).Y, pl[i].Size * scale, pl[i].paint);
					//canvas.DrawRect(pl[i].hitBox(getPlanetPoint(pl[i]).X-(pl[i].Size*scale), getPlanetPoint(pl[i]).Y-(pl[i].Size*scale),(float)pl[i].Size*scale,scale),pl[i].paint);
				}
			}
			if (theme.Equals("image"))           //disegno i pianeti come immagini stilizzate
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (openPopUp)
					{
						if (i == iPlanet)           //disegno solo il pianeta selezionato
							canvas.DrawBitmap(pl[i].texture, SKRect.Create((float)getPlanetPoint(pl[i]).X, (float)getPlanetPoint(pl[i]).Y, pl[i].Size * scale, pl[i].Size * scale), null);
					}
					else
						canvas.DrawBitmap(pl[i].texture, SKRect.Create((float)getPlanetPoint(pl[i]).X, (float)getPlanetPoint(pl[i]).Y, pl[i].Size * scale, pl[i].Size * scale), null);
				}
			}

			if (theme.Equals("imageHD"))            //disegno i pianeti come immagini reali
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (openPopUp)
					{
						if (i == iPlanet)
							canvas.DrawBitmap(pl[i].textureHD, SKRect.Create((float)getPlanetPoint(pl[i]).X, (float)getPlanetPoint(pl[i]).Y, pl[i].Size * scale, pl[i].Size * scale), null);
					}
					else
					{
						canvas.DrawBitmap(pl[i].textureHD, SKRect.Create((float)getPlanetPoint(pl[i]).X, (float)getPlanetPoint(pl[i]).Y, pl[i].Size * scale, pl[i].Size * scale), null);
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

				float cx = (float)getPlanetPoint(pl[0]).X + (pl[iPlanet].Size * scale) / 2;        //centro cerchio x
				float cy = (float)getPlanetPoint(pl[0]).Y + (pl[iPlanet].Size * scale) / 2;           //centro cerchio y
				float r = (float)Math.Sqrt(                                                                               //raggio
						Math.Pow((getPlanetPoint(pl[j]).Y + (pl[iPlanet].Size * scale) / 2) - (getPlanetPoint(pl[0]).Y + (pl[iPlanet].Size * scale) / 2), 2) +
						Math.Pow((getPlanetPoint(pl[j]).X + (pl[iPlanet].Size * scale) / 2) - (getPlanetPoint(pl[0]).X + (pl[iPlanet].Size * scale) / 2), 2)
						);
				double p = r * 2 * Math.PI;

				double x;
				if ( j < 5 )
					x = (72 * p) / 1620;    //costante grafica				//FARE UN RAPPORTO TRA LA DISTANZA DEL SOLE E '3240'
				else if( j == 5)
					x = (72 * p) / 2430;
				else
					x = (72 * p) / 3240;
				double test = ((360 / x) * scale);
				for (double i = 0; i < 360; i += test)
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

		public void drawPLanetData(SKCanvas canvas)
		{
			ScroolView.IsVisible = true;
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

		private Boolean restoreCamera = false;

		//-------------------------------------------------------------------------------------------------------------------\\
		//FUNZIONI TOCCO

		private Boolean clickedPlanet = false, openPopUp = false, joyStickVisible = false;
		private int iPlanet;
		private int dir = 0;        //da implementare
		private Boolean top = false, right = false, down = false, left = false;
		private int zoomGesture = 0;             //questa variabile può avere 3 stati [ 0 se non sto zommando ], [ 1 se ho finito di zoomare*], [ 2 se non sto zoommando]
													//*se ho finito di zoomare devo anche completare lo stato di movimento con variabili nulle cosi da resettare tutto 

		private void canvasView_Touch(object sender, SKTouchEventArgs e)
		{
			SKRect touchRect = SKRect.Create(e.Location.X, e.Location.Y, 1, 1);

			if (e.ActionType == SKTouchAction.Pressed && openPopUp == false && !touchRect.IntersectsWith(joyStick.GetRect()) &&
				!touchRect.IntersectsWith(switchJoyStick.GetRect()) && !touchRect.IntersectsWith(changeButton.GetRect()))
			{
				for (int i = 0; i < pl.Count; i++)
				{
					if (touchRect.IntersectsWith(pl[i].hitBox(getPlanetPoint(pl[i]).X, getPlanetPoint(pl[i]).Y, pl[i].Size, scale)))
					{
						oldScale = scale;
						clickedPlanet = true;
						iPlanet = i;
					}
				}
			}

			//funzioni bottoni personalizzati
			if (!openPopUp)
			{
				//cambio del tema dei pianeti
				if (touchRect.IntersectsWith(changeButton.GetRect()))
				{
					if (theme.Equals("image"))
						theme = "imageHD";
					else
						theme = "image";
					changeButton.switchTheme();
				}

				//clicco il pulsante per mostrare il joystick
				if (touchRect.IntersectsWith(switchJoyStick.GetRect()))
				{
					joyStickVisible = !joyStickVisible;
					switchJoyStick.changeStateOn();
				}

				//clicco il joystick
				//dir : 1=up, 2=right, 3=down, 4=left
				if (e.ActionType == SKTouchAction.Pressed && joyStickVisible)
				{
					if (touchRect.IntersectsWith(joyStick.getTop()))        //tasto sopra
					{
						top = true;
						dir = 1;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getDown()))           //sotto
					{
						down = true;
						dir = 3;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getRight()))         //destra
					{
						right = true;
						dir = 2;
						e.Handled = true;
					}
					if (touchRect.IntersectsWith(joyStick.getleft()))        //sinsitra
					{
						left = true;
						dir = 4;
						e.Handled = true;
					}
				}
			}

			//aperto il popup se clicco fuori da essi esco dalla modalità
			if (openPopUp && !touchRect.IntersectsWith(topPopUp) && !touchRect.IntersectsWith(downPopUp))
			{
				openPopUp = false;
				LabelPlanetname.IsVisible = false;
				ScroolView.IsVisible = false;

				restoreCamera = true;
				pl[iPlanet].Size = 70;         //ripristino le dimensioni del pianeta quando esco dalla visualizzazione popup
			}

			//rilascio la pressione su un tasto del joystick
			if (e.ActionType == SKTouchAction.Released)
			{
				top = false;
				right = false;
				down = false;
				left = false;
				dir = 0;
				e.Handled = false;
			}
		}

		private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)  //dito che preme e si muove in giro
		{
			if (!clickedPlanet && !openPopUp && !joyStickVisible && zoomGesture!=2)
			{
				if (e.StatusType.ToString() == "Running" && zoomGesture!=1)
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
				}else if(e.Status.ToString() == "Completed")
				{
					zoomGesture = 1;
				}


			}
		}

		//-------------------------------------------------------------------------------------------------------------------\\
		//FUNZIONI COORDINATE PIANETI

		private void setPositions()           //SETTO LE COORDINATE DI TUTTI I PIANETI
		{
			//SETTO IL SOLE
			pl[0].coord.X = 0;
			pl[0].coord.Y = 0;

			//SETTO LA TERRA
			pl[1].coord = setPlanet((int)pl[0].coord.X, (int)pl[0].coord.Y, (pl[0].RA + 180) % 360, pl[0].dist2D,1);

			//SETTO I PIANETI RIMANENTI
			for (int i = 2; i < pl.Count; i++)
				pl[i].coord = setPlanet((int)pl[1].coord.X, (int)pl[1].coord.Y, pl[i].RA, pl[i].dist2D, i);
		}

		private Point setPlanet(int x, int y, float RA, float dis2D, int i)   //SETTO UN PIANETA
		{
			float disPx = maxDistancePx * dis2D / maxDistanceKm;
			int deltaX = (int)(Math.Cos((Math.PI / 180) * RA) * disPx);
			int deltaY = (int)(Math.Sin((Math.PI / 180) * RA) * disPx);
			return new Point(x + deltaX, y + deltaY);
		}

		private Point getPlanetPoint(Planet pl)
		{
			Point po = new Point();

			po.X = pl.coord.X * scale +                               //la posizione iniziale del pianeta (che non viene mai cambiata!) zoommata  +
					width / 2 + (center.X - width / 2) * scale +      //il centro della telecamera zoommato (with / 2 non deve venir scalato però quindi lo tiro fuori)  +
					panPoint.X * scale;                               //panPoint è il movimento totale che ha fatto il dito mentre si sta spostando, see panGesture for more info

			po.Y = pl.coord.Y * scale +                         //stessa cosa ma per la Y
					height / 2 + (center.Y - height / 2) * scale +
					panPoint.Y * scale;

			return po;
		}


		public void loop()
		{
			canvasView.InvalidateSurface();
		}

		//-------------------------------------------------------------------------------------------------------------------\\
		//TEXTURE

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
	}
}