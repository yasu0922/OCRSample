using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;


namespace OCRSample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try{
                    WindowsOcr ocr = new WindowsOcr();
                    string path = args[0];
                    var result = ocr.Recognize(path);
                    foreach (var l in result.Lines)
                    {
                        Console.WriteLine(l.Text);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    internal class WindowsOcr
    {
        /// <summary>
        /// 指定した画像内に含まれる文字情報を取得します
        /// </summary>
        /// <param name="filename">画像ファイル名</param>
        /// <returns>OCR結果</returns>
        internal OcrResult Recognize(string filename)
        {
            Task<OcrResult> result = OcrMain(filename);
            result.Wait();
            return result.Result;
        }

        private async Task<OcrResult> OcrMain(string filename)
        {
            OcrEngine ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            var bitmap = await LoadImage(filename);
            var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
            return ocrResult;
        }

        private async Task<SoftwareBitmap> LoadImage(string path)
        {
            var fs = System.IO.File.OpenRead(path);
            var buf = new byte[fs.Length];
            fs.Read(buf, 0, (int)fs.Length);
            var mem = new MemoryStream(buf);
            mem.Position = 0;

            var stream = await ConvertToRandomAccessStream(mem);
            var bitmap = await LoadImage(stream);
            return bitmap;
        }

        private async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(memoryStream.ToArray()));
            task.Start();
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }

        private async Task<SoftwareBitmap> LoadImage(IRandomAccessStream stream)
        {
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return bitmap;
        }


    }
}
