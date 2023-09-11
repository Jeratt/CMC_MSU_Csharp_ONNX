using System;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;

using MyYOLOApi;

namespace YOLO_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";
            string? imagePath;
            Image<Rgb24> img;
            ModelManager modelManager = new ModelManager(modelPath);

            while (true)
            {
                imagePath = Console.ReadLine();
                if (imagePath is null || imagePath == "")
                    break;
                img = Image.Load<Rgb24>(imagePath);
                var results = modelManager.Predict(img);
            }
        }
    }
}
