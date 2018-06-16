using System;
using System.Collections.Generic;
using Colourful;

namespace MyImplementation
{
    public class Patch
    {
        public double Saliency { get; set; }
        public LabColor AverageColor;
        public int X, Y, ScaleIndex, NumberOfColors;
        public double ColorValue { get; private set; }


        public Patch(IList<LabColor> colors, int x, int y, int scaleIndex)
        {
            NumberOfColors = colors.Count;
            CalculateAverage(colors);
            X = x;
            Y = y;
            ScaleIndex = scaleIndex;
            CalculateColorValue();
        }

        public override string ToString()
        {
            return "|ScaleIndex: " + ScaleIndex + "  Saliency: " + Saliency + "|";
        }

        private void CalculateColorValue()
        {
            ColorValue = AverageColor.L + AverageColor.a + AverageColor.b;
        }

        private void CalculateAverage(IList<LabColor> colors)
        {
            double l = 0.0, a = 0.0, b = 0.0;
            double size = colors.Count;
            foreach(var color in colors)
            {
                l += color.L;
                a = color.a;
                b = color.b;
            }
            AverageColor = new LabColor(l / size, a / size, b / size);
        }
    }
}