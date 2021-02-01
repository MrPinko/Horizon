using SkiaSharp.Views.Forms;
using SkiaSharp;

namespace Horizon
{
    public class Star : Planet
    {   
        public Star(string name, double RA, double DEC, SKColor color, double dim) : base(name, (float)RA, (float)DEC, dim, color){}
    }
}
