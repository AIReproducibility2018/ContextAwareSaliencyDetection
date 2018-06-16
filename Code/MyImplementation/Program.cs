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
            var is14 = false;
            var dataset = new DataSet(is14);
            Console.Write("Images from: "+dataset.InputPath+"\n");
//            var inputPaths = GetPaths(dataset.InputPath).GetRange(0,1);
            var inputPaths = GetPaths(dataset.InputPath);
            Console.Write(inputPaths.Count + " image(s)\n");
            var index = 0.0;
            foreach (var inputPath in inputPaths)
            {
                Console.Write("\nProgress: " + (index/inputPaths.Count)*100 + "%");
                var temp = inputPath.Split('/');
                var imageName = temp[temp.Length - 1];
                var outputPath = dataset.OutputPath + imageName;
                Method method = new Method(inputPath, outputPath);
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