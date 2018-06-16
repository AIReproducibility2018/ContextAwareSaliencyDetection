using System.Collections.Generic;
using Colourful;

namespace MyImplementation
{
    public class SalientPixel
    {
        public LabColor Color { get; private set; }
        
        public Patch[] Patches { get; private set; }

        public double AvgSaliency;

        public readonly int X, Y;

        public SalientPixel(LabColor color, int x, int y, int numberOfPatches)
        {
            Color = color;
            X = x;
            Y = y;
            Patches = new Patch[numberOfPatches];
        }

        public void AddPatches(List<List<LabColor>> pixels)
        {
            for (var i = 0; i < pixels.Count; i++)
            {
                Patches[i] = new Patch(pixels[i], X, Y, i);
            }
        }

        public override string ToString()
        {
            return "Pixel[" + X + "][" + Y + "] AvgSaliency: " + AvgSaliency + "  Patches: {" + Patches[0] + " , " +
                   Patches[1] + " , " + Patches[2] + " , " + Patches[3] + "}";
        }
    }
}