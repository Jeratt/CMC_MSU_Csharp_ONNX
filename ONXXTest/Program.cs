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
        static CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";
            string? imagePath;

            Image<Rgb24> img;
            List<Pair> res = new List<Pair>();
            List<ObjectBox> lob;
            List<Tuple<Task<List<ObjectBox>>, string>> tasks = new List<Tuple<Task<List<ObjectBox>>, string>>();
            FileManager fm = new FileManager();

            ModelManager modelManager = new ModelManager(modelPath, fm);

            while (true)
            {
                imagePath = Console.ReadLine();
                if (imagePath is null || imagePath == "")
                    break;
                if (imagePath == "cancel")
                {
                    cts.Cancel();
                    continue;
                }
                img = Image.Load<Rgb24>(imagePath);
                //lob = await modelManager.PredictAsync(img);
                try
                {
                    tasks.Add(new Tuple<Task<List<ObjectBox>>, string>(modelManager.PredictAsync(img, cts.Token), imagePath));
                }
                catch(Exception x)
                {
                    Console.WriteLine(x.Message);
                }
/*                foreach (ObjectBox obj in lob)
                {
                    res.Add(new Pair(imagePath, obj));
                }*/
            }
            foreach(var t in tasks)
            {
                try
                {
                    imagePath = t.Item2;
                    lob = await t.Item1;
                    if (lob is null)
                    {
                        continue;
                    }
                    foreach (ObjectBox? obj in lob)
                    {
                        res.Add(new Pair(imagePath, obj));
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            ObjectBoxToCSV(res, "results.csv");
            if (File.Exists("results.csv"))
            {
                Console.WriteLine(Path.GetFullPath("results.csv"));

                FileStream fs = File.OpenRead("results.csv");
                StreamReader reader = new StreamReader(fs);
                Console.WriteLine(reader.ReadToEnd());
            }
        }

        static void ObjectBoxToCSV(List<Pair> pairs, string filename)
        {
            FileStream fs = null;
            StreamWriter writer = null;
            try
            {
                fs = File.Create(filename);
                writer = new StreamWriter(fs);
                writer.WriteLine("filename,class,x,y,w,h");
                foreach (var p in pairs)
                {
                    writer.WriteLine($"{p.Filename},{p.Obj.Class},{p.Obj.XMin},{p.Obj.YMax},{p.Obj.XMax - p.Obj.XMin},{p.Obj.YMax - p.Obj.YMin}");
                }
            }
            catch (Exception x)
            {
                Console.WriteLine($"ERROR SAVING FILE ! ! !: {x}");
                //throw;
                return;
            }
            finally
            {
                writer?.Dispose();
                fs?.Close();
            }
        }
    }

    class Pair
    {
        public ObjectBox Obj { get; set; }
        public string Filename { get; set; }
        public Pair(string filename, ObjectBox ob)
        {
            Filename = filename;
            Obj = ob;
        }
    }

    class FileManager : IFileManager
    {
        public bool CheckIfExists(string path)
        {
            return File.Exists(path);
        }

        public void PrintText(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }
    }
}
