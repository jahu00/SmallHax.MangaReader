using SkiaSharp;
using SmallHax.Utils;
using System.Reflection;

namespace SmallHax.MangaReader.Services
{
    public class EmbeddedFontService : AManager<SKFont>, ISKFontManager
    {
        protected override SKFont Initialize(string key)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources.Fonts." + key);
            var typeface = SKTypeface.FromStream(stream);
            var font = new SKFont { Typeface = typeface };
            return font;
        }
    }
}
