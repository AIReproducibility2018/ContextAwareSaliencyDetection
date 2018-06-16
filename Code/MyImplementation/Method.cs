using System;
using System.Linq;
using Colourful;

namespace MyImplementation
{
    public class Method
    {

        public Method(string inputPath, string outputPath)
        {
//            var scales = new double[]{0.3, 0.5, 0.8, 1.0};
            var scales = new double[]{0.25, 0.5, 0.75, 1.0};
            //_scales = new double[] {0.3};
//            for (var i = 18; i < 100; i+= 15)
//            {
//                var scaledImage = new ScaledImage(inputPath, scales, i, false);
//                scaledImage.WriteSaliency(outputPath+i+".jpg");
//            }
            var scaledImage = new ScaledImage(inputPath, scales, 4);
            scaledImage.WriteSaliency(outputPath);
        }
    }
}