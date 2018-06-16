using System;
using System.Linq;
using Colourful;

namespace MyImplementation
{
    public class Method
    {

        public Method(string inputPath, string outputPath)
        {
            var scales = new double[]{0.25, 0.5, 0.75, 1.0};
            var scaledImage = new ScaledImage(inputPath, scales, 4);
            scaledImage.WriteSaliency(outputPath);
        }
    }
}