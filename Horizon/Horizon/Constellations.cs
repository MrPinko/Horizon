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

            ArrayList singleStarIds = new ArrayList();  //array con le stelle ma senza le copie

            for (int i = 0; i < starIds.Length; i++)
                if (!singleStarIds.Contains(starIds[i]))
                    singleStarIds.Add(starIds[i]);

            for (int i = 0; i < singleStarIds.Count; i++)
            {
                Planet temp = stars[(int)singleStarIds[i] - 1];
                medRA += temp.RA;
                medDEC += temp.DEC;
            }

            medRA /= singleStarIds.Count;
            medDEC /= singleStarIds.Count;

            //controllo in modo brutto se il nome della costellazione è molto lontano dalla costellazione
            if (Math.Abs(medRA - stars[(int)singleStarIds[1]].RA) > 60 || Math.Abs(medDEC - stars[(int)singleStarIds[1]].DEC) > 60)
            {
                medRA = stars[(int)starIds[(int)(starIds.Length / 2)]].RA;
                medDEC = stars[(int)starIds[(int)(starIds.Length / 2)]].DEC;
            }

            costName = new Planet(medRA, medDEC);
        }
    }

}
