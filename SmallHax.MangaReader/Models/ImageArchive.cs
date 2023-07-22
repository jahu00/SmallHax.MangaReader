using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallHax.MangaReader.Models
{
    public class ImageArchive
    {
        private static readonly string[] allowedExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp" };
        private ZipArchive archive;
        private List<string> pageFileNames;
        public string FileName { get; private set; }
        public int PageCount => pageFileNames.Count;

        public static ImageArchive FromFileName(string fileName)
        {
            var archive = ZipFile.OpenRead(fileName);
            var pageFileNames = archive.Entries.Select(x => x.FullName).Where(x => allowedExtensions.Any(extension => x.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))).Order().ToList();

            var result = new ImageArchive
            {
                archive = archive,
                pageFileNames = pageFileNames
            };
            return result;
        }

        public SKImage GetPage(int i)
        {
            return GetPage(i, out var _);
        }

        public SKImage GetPage(int i, out string pageFileName)
        {
            pageFileName = pageFileNames[i];
            var image = GetPageByName(pageFileName);
            return image;
        }

        private SKImage GetPageByName(string pageFileName)
        {
            var entry = archive.GetEntry(pageFileName);
            var stream = entry.Open();
            var image = SKImage.FromEncodedData(stream);
            return image;
        }

        public List<SKImage> GetPages()
        {
            var images = pageFileNames.Select(x => GetPageByName(x)).ToList();
            return images;
        }
    }
}
