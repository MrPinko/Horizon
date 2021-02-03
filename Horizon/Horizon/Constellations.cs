using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Horizon
{
    public class Constellations
    {
        public Constellation[] cons;
        public bool printText;
        private Camera3D cam;
        private List<Planet> stars;
        public Constellations(Camera3D cam, List<Planet> stars)
        {
            this.cons = ConstellationsDB.getAll();
            this.cam = cam;
            this.stars = stars;
            this.printText = true;
        }

        private double maxX = 0;
        private double minX = 0;
        private double maxY = 0;
        private double minY = 0;
        private SKPoint a, b;
        private bool drawText = false;

        public void drawAll(SKCanvas canvas)
        {
            foreach (Constellation constellation in this.cons)
            {
                for (int i = 0; i < constellation.starIds.Length; i += 2)
                {
                    a = cam.toScreen(stars[constellation.starIds[i] - 1]);
                    b = cam.toScreen(stars[constellation.starIds[i + 1] - 1]);
                    if (Math.Abs(a.X - b.X) > 500 || Math.Abs(a.Y - b.Y) > 500)
                    {
                        drawText = false;
                        break;
                    }
                    if (printText) //per la mediana
                    {
                        if (i == 0) { maxX = a.X; minX = a.X; maxY = a.Y; minY = a.Y; }
                        if (a.X > maxX) maxX = a.X;
                        if (a.X < minX) minX = a.X;
                        if (b.X > maxX) maxX = b.X;
                        if (b.X < minX) minX = b.X;
                        if (a.Y > maxY) maxY = a.Y;
                        if (a.Y < minY) minY = a.Y;
                        if (b.Y > maxY) maxY = b.Y;
                        if (b.Y < minY) minY = b.Y;
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
                    canvas.DrawText(constellation.code, (float)((maxX + minX) / 2), (float)((maxY + minY) / 2), cam.constellationPaint);
            }
        }
    }

    public class Constellation
    {
        public string code;
        public int[] starIds;
        public Constellation( string code, int[] starIds )
        {
            this.code = code;
            this.starIds = starIds;
        }

    }
}
