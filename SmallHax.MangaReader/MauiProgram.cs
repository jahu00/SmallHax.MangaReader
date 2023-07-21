using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Handlers;
using SmallHax.MangaReader.Controls;
using SmallHax.MangaReader.Services;

namespace SmallHax.MangaReader;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<ISKFontManager, EmbeddedFontService>();
        builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

		builder.ConfigureMauiHandlers(h =>
         {
             h.AddHandler<SKLabel, SKCanvasViewHandler>();
             h.AddHandler<ImageRenderer, SKCanvasViewHandler>();
         });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
	}
}
