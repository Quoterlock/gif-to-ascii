using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace GifToAscii
{
    class Program
    {
        const string _asciiChars = " .,:;+*D#@";
        const int _height = 50;

        public static async Task Main(string[] args)
        {
            var path = "test.gif";
            if (args.Length == 1)
                path = args[0];

            var gif = await ImportGif(path);
            var compressedGif = Compress(gif, _height);
            var acsii = ConvertToAscii(compressedGif);
            while (true)
            {
                PlayAnimation(acsii);
            }
        }

        private static void PlayAnimation(List<string> ascii)
        {
            const int bufferSize = 16384;
            using var input = new StreamReader(
                Console.OpenStandardInput(),
                bufferSize: bufferSize);
            using var output = new StreamWriter(
                Console.OpenStandardOutput(),
                bufferSize: bufferSize);

            foreach (var frame in ascii)
            {
                // Write the new frame
                output.WriteLine(frame);
                output.Flush();
                // reset position
                Console.SetCursorPosition(0, Console.CursorTop - _height);

                // Delay for animation effect
                Task.Delay(100).Wait();
            }
        }

        private static List<string> ConvertToAscii(List<Image<Rgba32>> gif)
        {
            var frames = new List<string>();
            foreach(var frame in gif)
            {
                frames.Add(ConvertImageToAscii(frame));
            }
            return frames;
        }

        private static string ConvertImageToAscii(Image<Rgba32> frame)
        {
            var frameStr = new StringBuilder();
            for (int y = 0; y < frame.Height; y++)
            {
                for (int x = 0; x < frame.Width; x++)
                {
                    var pixel = frame[x, y];

                    // Calculate brightness
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3;
                    char asciiChar = GetAsciiChar((byte)brightness);
                    frameStr.Append(asciiChar);
                }
                frameStr.Append('\n');
            }
            return frameStr.ToString();
        }

        private static List<Image<Rgba32>> Compress(List<Image<Rgba32>> gif, int height)
        {
            var aspectRatio = 2.0;
            var width = height;
            if(gif.Count > 0)
                width = (int)(height * gif[0].Width / gif[0].Height * aspectRatio);


            var result = new List<Image<Rgba32>>(gif);
            foreach(var frame in result)
                frame.Mutate(x => x.Resize(width, height));
            return result;
        }

        private static async Task<List<Image<Rgba32>>> ImportGif(string path)
        {
            var frames = new List<Image<Rgba32>>();
            using (var image = await Image.LoadAsync<Rgba32>(path))
            {
                for(var i = 0; i < image.Frames.Count; i++)
                {
                    frames.Add(image.Frames.ExportFrame(i));
                }
            }
            return frames;
        }

        private static char GetAsciiChar(byte brightness)
        {
            int index = brightness * (_asciiChars.Length - 1) / 255;
            return _asciiChars[index];
        }
    }
}