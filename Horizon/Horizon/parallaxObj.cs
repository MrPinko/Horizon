using SkiaSharp;

namespace Horizon
{
    class parallaxObj
    {
        public SKPoint cp;
        public double paral;
        public int colorIndex;
        
        public parallaxObj(float x, float y, double paral, int colorIndex)
        {
            this.cp = new SKPoint(x, y);
            this.paral = paral;
            this.colorIndex = colorIndex;
        }

    }
}
