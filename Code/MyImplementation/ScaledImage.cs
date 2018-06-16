using System;
using System.Collections.Generic;
using System.Linq;
using Colourful;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using Colourful.Conversion;

namespace MyImplementation
{
    public class ScaledImage
    {
        private static readonly double MaxColorDistance = 100 * Math.Sqrt(3);
        private SalientPixel[][] _pixels;  // [x][y]
        private int _height;
        private int _width;
        private int _largestDimension;
        private const int C = 3;
        private const int K = 64;
        public int NumberOfPixels;
        private readonly bool _showMsg;
        private readonly int[] _scales;
        private readonly int _numberOfScales;
        private List<Patch> _patches;
        private HashSet<SalientPixel> attendedPoints;
        private double _gaussianHorizontalVariance;
        private double _gaussianVerticalVariance;
        private double _middleHeight;
        private double _middleWidht;
        private double _maxAvg = -1.0;
        private Random _random;


        public ScaledImage(string path, double[] scale, int maxScale=100, bool showMsg=true)
        {
            attendedPoints = new HashSet<SalientPixel>();
            _showMsg = showMsg;
            _numberOfScales = scale.Length;
            _scales = new int[_numberOfScales];
            _patches = new List<Patch>();
            _random = new Random(0);
            for (var i = 0; i < scale.Length; i++)
            {
                _scales[i] = (int) Math.Round(maxScale * scale[i]);
            }
            Array.Sort(_scales);
            Array.Reverse(_scales);
//            Console.WriteLine(string.Join(",", _scales));
            var resizedImage = ReadAndResizeImage(path);
            _gaussianHorizontalVariance = _width / 6.0;
            _gaussianVerticalVariance = _height / 6.0;
            _middleHeight = _height / 2.0;
            _middleWidht = _width / 2.0;
            WritePixels(resizedImage);
            CreatePatches();
            SortPatches();
            var maximumsR = CalculateSaliencyR();
            //PrintSomeValues();
            FindAttendedPoints(maximumsR);
            CalculateSaliencyAvg(maximumsR);
            //PrintSomeValues();
        }

        private void PrintSomeValues()
        {
            Console.Write("\n-------------\n" + _pixels[111][111] + "\n" + _pixels[138][138] + "\n" + _pixels[125][125] + "\n");
        }
        
        private Bitmap ReadAndResizeImage(string path)
        {
            if (_showMsg) Console.Write("\nReading Image...");
            var image = new Bitmap(path);
            var largestDimension = image.Width > image.Height ? image.Width : image.Height;
            var ratio = 250.0 / largestDimension;
            var resizedImage =
                ResizeImage(image, (int) Math.Round(image.Width * ratio), (int) Math.Round(image.Height * ratio));
            _height = resizedImage.Height;
            _width = resizedImage.Width;
            _largestDimension = _height > _width ? _height : _width;
            NumberOfPixels = _width * _height;
            if (_showMsg) Console.Write("\tImage Read");
            return resizedImage;
        }

        private void WritePixels(Bitmap resizedImage)
        {
            if (_showMsg) Console.Write("\tConverting Pixels...");
            var converter = new ColourfulConverter();
            _pixels = new SalientPixel[resizedImage.Width][];
            for (var x = 0; x < resizedImage.Width; x++)
            {
                _pixels[x] = new SalientPixel[resizedImage.Height];
                for (var y = 0; y < resizedImage.Height; y++)
                {
                    var pixelColor = resizedImage.GetPixel(x, y);
                    var input = new RGBColor(pixelColor.R / 255.0, pixelColor.G / 255.0, pixelColor.B / 255.0);
//                    if (pixelColor.R != 0 || pixelColor.G != 0 || pixelColor.B != 0)
//                    {
//                        Console.WriteLine(x+", "+y+" "+" "+pixelColor.R + "\t" + pixelColor.G + "\t" + pixelColor.B);
//                    }
                    _pixels[x][y] = new SalientPixel(converter.ToLab(input), x, y, _numberOfScales);
                }
            }
            if (_showMsg) Console.Write("\tPixels Converted");
        }

        private void CreatePatches()
        {
            if(_showMsg)Console.Write("\tCreating Patches...");

            for (var colIndex = 0; colIndex < _pixels.Length; colIndex++)
            {
                for (var rowIndex = 0; rowIndex < _pixels[colIndex].Length; rowIndex++)
                {
                    var maxShift = _scales.Max();
//                    Console.WriteLine("\n\n["+colIndex+","+rowIndex+"] - Maxshift: "+maxShift);
                    
                    var tempPatches = new List<List<LabColor>>();
                    for (var i = 0; i < _scales.Length; i++)
                    {
                        tempPatches.Add(new List<LabColor>());
                    }
                    for (var colShift = -maxShift; colShift <= maxShift; colShift++)
                    {
                        var col = colIndex + colShift;
                        if(!XWithin(col))
                        {
                            continue;
                        }

                        for (var rowShift = -maxShift; rowShift <= maxShift; rowShift++)
                        {
                            var row = rowIndex + rowShift;
                            if (!YWithin(row))
                            {
                                continue;
                            }
                            tempPatches[0].Add(_pixels[col][row].Color);
//                            Console.Write(" ["+col+","+row+"]");
                            for (var i = 1; i < _numberOfScales; i++)
                            {
                                if (Math.Abs(rowShift) <= _scales[i] && Math.Abs(colShift) <= _scales[i])
                                {
                                    tempPatches[i].Add(_pixels[col][row].Color);
                                }
                            }
                        }
                    }
                    _pixels[colIndex][rowIndex].AddPatches(tempPatches);
                    _patches.AddRange(_pixels[colIndex][rowIndex].Patches);
                }
            }
            if(_showMsg)Console.Write("\tPatches Created");
        }

        private void SortPatches()
        {
            
            if(_showMsg)Console.Write("\tSorting Patches...");
//            for (var i = 1; i < _patches.Count; i++)
//            {
//                var patch = _patches[i];
//                if (patch.ColorValue != 0)
//                {
//                    Console.WriteLine("[" + patch.X + ", " + patch.Y + "] ScaleIndex: " + patch.ScaleIndex + "\t" +
//                                      patch.AverageColor + "\t" + patch.NumberOfColors + "\t\t\t" + ReferenceEquals(patch, _patches[i-1]));
//                }
//            }
            _patches = _patches.OrderBy(v => v.ColorValue).ToList();

            if(_showMsg)Console.Write("\tPatches Sorted");
        }
        

        private List<double> CalculateSaliencyR()
        {
            if(_showMsg)Console.Write("\tCalculating Saliency Sum...");
            var maximums = _scales.Select(s => -1.0).ToList();
            var invertedK = 1.0 / K;

            for (var currentPatch = 0; currentPatch < _patches.Count; currentPatch++)
            {
                var distances = new List<Tuple<double, double>>();

                if (_patches[currentPatch].X == _patches[currentPatch].Y &&
                    (_patches[currentPatch].Y == 111 || _patches[currentPatch].Y == 125 ||
                     _patches[currentPatch].Y == 138))
                {
                    Console.Write("");
                }
                
//                //todo Add check for other from same pixel
                for (var i = currentPatch-1; i >= currentPatch - K*2.0 && i >= 0; i--)
                {
                    if (_patches[currentPatch].X == _patches[i].X && _patches[currentPatch].Y == _patches[i].Y)
                    {
                        continue;
                    }
                    var distance = D(_patches[currentPatch], _patches[i]);
                    var colDist = DColor(_patches[currentPatch].AverageColor, _patches[i].AverageColor);
                    distances.Add(new Tuple<double, double>(colDist, distance));
                }

                for (var i = currentPatch + 1; i <= currentPatch + K*2.0 && i < _patches.Count; i++)
                {
                    if (_patches[currentPatch].X == _patches[i].X && _patches[currentPatch].Y == _patches[i].Y)
                    {
                        continue;
                    }
                    var distance = D(_patches[currentPatch], _patches[i]);
                    var colDist = DColor(_patches[currentPatch].AverageColor, _patches[i].AverageColor);
                    distances.Add(new Tuple<double, double>(colDist, distance));
                }
                

                distances.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                //distances.Reverse();
                var sum = 0.0;
                for (var i = 0; i < K; i++)
                {
                    sum += distances[i].Item2;
                }
//                if (sum != 0)
//                {
//                    Console.Write("");
//                }
                _patches[currentPatch].Saliency = 1.0 - Math.Exp(-invertedK * sum);
                maximums[_patches[currentPatch].ScaleIndex] = Math.Max(maximums[_patches[currentPatch].ScaleIndex], _patches[currentPatch].Saliency);
//                if (_patches[currentPatch].Saliency > 0)
//                {
//                    Console.WriteLine("["+_patches[currentPatch].X+","+_patches[currentPatch].Y+"] - " +  _patches[currentPatch].Saliency);
//                }
                
            }
            if(_showMsg)Console.Write("\tSaliency Sum Calculated");
            return maximums;
        }

        private List<double> CalculateSaliencyROLD()
        {
            if(_showMsg)Console.Write("\tCalculating Saliency Sum...");

            var colours = new List<List<double>>();
            var distances = new List<List<double>>();
            var maximums = _scales.Select(s => -1.0).ToList();

            for (var i = 0; i < _patches.Count; i++)
            {
                colours.Add(new List<double>());
                distances.Add(new List<double>());
                var maxDistance = -1.0;
                var positive = true;
                var changeDirection = true;
                var increase = false;
                var diff = 1;
                for (var j = 0; j < K; j++)
                {
                    var index = -1;
                    if (positive)
                    {
                        index = i + diff;
                        if (index >= _patches.Count)
                        {
                            changeDirection = false;
                            positive = false;
                            index = i - diff;
                        }
                    }
                    else
                    {
                        index = i - diff;
                        if (index < 0)
                        {
                            changeDirection = false;
                            positive = true;
                            index = i + diff;
                        }
                    }

                    if (_patches[index].X == _patches[i].X && _patches[index].Y == _patches[i].Y)
                    {
                        j--;
                    }
                    else
                    {
                        
                        //sum += D(_patches[index], _patches[i]);
                        var colourDist = DColor(_patches[index].AverageColor, _patches[i].AverageColor);
                        colours[i].Add(colourDist);
                        maxDistance = maxDistance > colourDist ? maxDistance : colourDist;
                        distances[i].Add(DPosition(_patches[index], _patches[i]));
                    }
                    if (increase)
                    {
                        diff++;
                    }
                    increase = !increase;
                    if (changeDirection)
                    {
                        positive = !positive;
                    }

                }

                //_patches[i].Saliency = 1.0 - Math.Exp(-(1.0 / K) * sum);
            }

            for (var i = 0; i < _patches.Count; i++)
            {
                var sum = 0.0;
                for (var index = 0; index < colours[i].Count; index++)
                {
                    var colourDist = colours[i][index];
                    var dist = distances[i][index];
                    sum += D(colourDist, dist);
                }

                sum = sum / colours[i].Count;
                _patches[i].Saliency = 1.0 - Math.Exp(-(1.0 / K) * sum);
                maximums[_patches[i].ScaleIndex] = Math.Max(maximums[_patches[i].ScaleIndex], _patches[i].Saliency);
            }
            if(_showMsg)Console.Write("\tSaliency Sum Calculated");
            return maximums;
        }

        private void FindAttendedPoints(List<double> maximums)
        {
            if(_showMsg)Console.Write("\tFinding Attendended Points...");
            foreach (var patch in _patches)
            {
                if (patch.Saliency / maximums[patch.ScaleIndex] >= 0.8)
                {
                    attendedPoints.Add(_pixels[patch.X][patch.Y]);
                }
            }
            if(_showMsg)Console.Write("\tAttended Points found");
        }
        
        private void CalculateSaliencyAvg(List<double> maximums)
        {
            
            if(_showMsg)Console.Write("\tCalculating Avg Saliency...");
            double maxSize = _height * _width;
            foreach (var pixelRow in _pixels)
            {
                foreach (var pixel in pixelRow)
                {
                    var distanceToAttended = 10000000.0;
                    foreach (var attendedPoint in attendedPoints)
                    {
                        var distance = EuclideanDistance(pixel.X, pixel.Y, attendedPoint.X, attendedPoint.Y);
                        distanceToAttended = distance < distanceToAttended ? distance : distanceToAttended;
                    }

                    var ratio = 1.0 - distanceToAttended / maxSize;
//                    var ratio = 1.0;
                    var sum = pixel.Patches.Sum(patch => (patch.Saliency / maximums[patch.ScaleIndex] )*ratio);
//                    var sum = pixel.Patches[1].Saliency;
                    pixel.AvgSaliency = ((1.0 / pixel.Patches.Length) * sum) /** Gaussian(pixel.X, pixel.Y)*/;
                    _maxAvg = Math.Max(pixel.AvgSaliency, _maxAvg);
                }
            }
            if(_showMsg)Console.Write("\tAvg Saliency Calculated");
        }

        private double EuclideanDistance(int pixelX, int pixelY, int attendedPointX, int attendedPointY)
        {
            return Math.Sqrt(Math.Pow(pixelX - attendedPointX, 2) + Math.Pow(pixelY - attendedPointY, 2));
        }

        private bool XWithin(int x)
        {
            return x >= 0 && x < _width;
        }

        private bool YWithin(int y)
        {
            return y >= 0 && y < _height;
        }

        private double D(Patch a, Patch b)
        {
            return DColor(a.AverageColor, b.AverageColor) / (1 + C * DPosition(a, b));
        }
        
        private double D(double colourDist, double distance)
        {
            return colourDist / (1 + C * distance);
        }

        private static double DColor(LabColor c1, LabColor c2)
        {
//            return Math.Sqrt(
//                       Math.Pow((c1.a - c2.a)/100.0, 2)  +
//                       Math.Pow((c1.b - c2.b)/100.0, 2) +
//                       Math.Pow((c1.L - c2.L) /100.0, 2)
//                   );
//            return Math.Sqrt(
//                Math.Pow((c1.a - c2.a), 2)  +
//                Math.Pow((c1.b - c2.b), 2) +
//                Math.Pow((c1.L - c2.L), 2)
//            ) / MaxColorDistance;
            return Math.Sqrt(
                       Math.Pow((c1.a - c2.a), 2)  +
                       Math.Pow((c1.b - c2.b), 2) +
                       Math.Pow((c1.L - c2.L), 2)
                   );
        }

        private double DPosition(Patch a, Patch b)
        {
            return Math.Sqrt(
                       Math.Pow(a.X - b.X, 2) +
                       Math.Pow(a.Y - b.Y, 2)
                   ) / _largestDimension;
        }

        private double Gaussian(int x, int y)
        {
            var amplitude = 1.0;
            return amplitude * Calculate2DGaussian(x, y);
//            return CalculateGaussian(x, _middleWidht, amplitude, _gaussianHorizontalVariance) *
//                   CalculateGaussian(y, _middleHeight, amplitude, _gaussianVerticalVariance);
        }

        private double Calculate2DGaussian(double x, double y)
        {
            return Math.Exp(-
                      (Math.Pow(x - _middleWidht, 2) / 
                      (2* Math.Pow(_gaussianHorizontalVariance, 2)) +
                      (Math.Pow(y - _middleHeight, 2) / 
                      (2*Math.Pow(_gaussianVerticalVariance, 2)))
                       )
                    );
        }

        private double CalculateGaussian(double x, double b, double a, double c)
        {
            var r = a * Math.Exp(-Math.Pow(x - b, 2) / (2 * Math.Pow(c, 2)));
            
            return r;
        }
        
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public void WriteSaliency(string outputPath, bool useAlternativeMap = false, List<double> maximums = null)
        {
//            for (var i = 0; i < _pixels.Length; i++)
//            {
//                var pixelRow = _pixels[i];
//                for (var index = 0; index < pixelRow.Length; index++)
//                {
//                    var pixel = pixelRow[index];
//                    if (pixel.AvgSaliency != 0)
//                    {
//                        Console.WriteLine(pixel.X+", "+pixel.Y+"\t"+pixel.AvgSaliency);
//                    
//                    }
//                }
//            }
            
            var image = useAlternativeMap ? CreateSaliencyMap2(maximums) : CreateSaliencyMap();
            image.Save(outputPath);
        }
        
        private Image CreateSaliencyMap()
        {

            //create a new image of the right size
            Bitmap img = new Bitmap(_width, _height);
            //double maxSaliencyValue = saliencyValues.Max();
            //double minSaliencyValue = saliencyValues.Min();
            
            //Console.Write("\n\n\n"+maxSaliency+"\t\t"+minSaliency+"\n\n\n");

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var scaledSaliency = _pixels[x][y].AvgSaliency / _maxAvg;
                    var color = GetColor(scaledSaliency);
                    if (x == 111 && y == 111 || x == 138 && y == 138 || x == 125 && y == 125)
                    {
                        Console.WriteLine(x+","+y+" - "+scaledSaliency+"\t"+color);
                    }
                        //_pixels[110][110] + "\n" + _pixels[137][137] + "\n" + _pixels[124][124]
//                    var color = GetColor(_pixels[x][y].AvgSaliency);
                    img.SetPixel(x, y, color);
                    
                }
            }

            return img;

        }
        
        private Image CreateSaliencyMap2(List<double> maximums = null)
        {
            Bitmap img = new Bitmap(_width, _height);
            
            var converter = new ColourfulConverter();

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
//                    var color = _pixels[x][y].Patches[0].ColorValue > 0
//                        ? converter.ToRGB(_pixels[x][y].Patches[0].AverageColor)
//                        : Color.AliceBlue;
                    var scaledSaliency = _pixels[x][y].Patches[0].Saliency / maximums[0];
                    var color = GetColor(scaledSaliency);
                    img.SetPixel(x, y, color);
                    
                }
            }

            return img;

        }

        private Color GetColor(double saliency)
        {
            var val = (int) Math.Round(255.0 * saliency);
            return Color.FromArgb(255, val, val, val);
        }
    }
    
    
}