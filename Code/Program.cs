using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace MyImplementation
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var inputPath = "../../Input/";
            var outputPath = "../../Output/";
            var inputPaths = GetPaths(inputPath);
            Console.Write(inputPaths.Count + " image(s)\n");
            var index = 0.0;
            foreach (var inputImagePath in inputPaths)
            {
                Console.Write("\nProgress: " + (index/inputPaths.Count)*100 + "%");
                var temp = inputImagePath.Split('/');
                var imageName = temp[temp.Length - 1];
                var outputImagePath = outputPath + imageName;
                Method method = new Method(inputImagePath, outputImagePath);
                index++;
            }
            Console.Write("\nCompleted");
        }

        private static List<string> GetPaths(string folderName)
        {
            return Directory.GetFiles(folderName, "*.*", SearchOption.AllDirectories)
                .ToList();
        }
    }
}