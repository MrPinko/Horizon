using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Horizon
{
    public class Constellations
    {
        public Constellation[] cons;
        public bool printText;
        private Camera3D cam;
        public List<Planet> stars;
        private SKPoint a, b;
        private bool drawText = false;

        public Constellations(Camera3D cam, List<Planet> stars)
        {
            this.cam = cam;
            this.stars = stars;
            this.printText = true;
            this.cons = ConstellationsDB.getAll(this.stars);
        }

        public void drawAll(SKCanvas canvas)
        {
            //foreach (Constellation constellation in this.cons)
            for(int i = 0; i < cons.Length; i++)
            {
                for (int j = 0; j < cons[i].starIds.Length; j += 2)
                {
                    a = cam.toScreen(stars[cons[i].starIds[j] - 1]);
                    b = cam.toScreen(stars[cons[i].starIds[j + 1] - 1]);

                    if (Math.Abs(a.X - b.X) > 750 || Math.Abs(a.Y - b.Y) > 750)
                    {
                        drawText = false;
                        break;
                    }
                    //quando una costellazione è sull'orlo del cerchio creato dalla trasformazione
                    //in piano della sfera le linee andrebbero da una parte all'altra del cerchio,
                    //quindi se i due punti sono troppo lontani, non li collego
                    //QUESTA COSA SI BUGGA SE METTEREMO LO ZOOM
                    canvas.DrawLine(a, b, cam.constellationPaint);
                }
                if (!drawText)
                {
                    drawText = true;
                    continue;
                }
                if (printText)
                {
                    SKPoint temp = cam.toScreen(cons[i].costName);
                    canvas.DrawText(cons[i].code.ToUpper(), temp.X, temp.Y, cam.constellationPaint);
                }
            }
        }
    }

    public class Constellation
    {
        public string code;
        public int[] starIds;
        public Planet costName;     //pianeta che sta nella posizione del nome della costellazione
        public List<Planet> stars;
        float medRA, medDEC;                //RA e DEC di dove stampare il nome

        public Constellation(List<Planet> stars, string code, int[] starIds )
        {
            this.stars = stars;
            this.code = code;
            this.starIds = starIds;
            medRA = 0;
            medDEC = 0;
            double x = 0, y = 0, z = 0, Hyp;
            ArrayList singleStarIds = new ArrayList();      //array con le stelle ma senza le copie

            for (int i = 0; i < starIds.Length; i++)        //creo l'array senza le copie
                if (!singleStarIds.Contains(starIds[i]))
                    singleStarIds.Add(starIds[i]);

            for (int i = 0; i < singleStarIds.Count; i++)   //sommo le coordinate (i - 1 serve perchè la prima stella nell'array ha come id 1)
            {
                x += Math.Cos(Misc.toRad(stars[(int)singleStarIds[i] - 1].DEC)) * Math.Cos(Misc.toRad(stars[(int)singleStarIds[i] - 1].RA));
                y += Math.Cos(Misc.toRad(stars[(int)singleStarIds[i] - 1].DEC)) * Math.Sin(Misc.toRad(stars[(int)singleStarIds[i] - 1].RA));
                z += Math.Sin(Misc.toRad(stars[(int)singleStarIds[i] - 1].DEC));
            }
            x /= singleStarIds.Count;                       //calcolo la media
            y /= singleStarIds.Count;
            z /= singleStarIds.Count;

            Hyp = Math.Sqrt(x * x + y * y);                     //converto in RA e DEC
            medRA = Misc.toDeg((float)Math.Atan2(y, x));    
            medDEC = Misc.toDeg((float)Math.Atan2(z, Hyp));

            if (medDEC > 0 && medDEC < 2)                    //se è sulla riga dell'equatore la sposto un po
                medDEC = 3;
            else if(medDEC < 0 && medDEC > -2)
                medDEC = -3;

            costName = new Planet(medRA, medDEC);
        }
    }

}
