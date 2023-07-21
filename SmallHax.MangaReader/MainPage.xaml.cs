using SmallHax.MangaReader.Models;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace SmallHax.MangaReader;

public partial class MainPage : ContentPage
{
    private ImageArchive imageArchive;

    private Direction readingDirection = Direction.LeftToRight;

    private int pageIndex = 0;

    public MainPage()
	{
		InitializeComponent();
	}

    private async void ManuItem_Tapped(object sender, TappedEventArgs e)
    {
        var result = await FilePicker.PickAsync(PickOptions.Default);
        if (result == null)
        {
            return;
        }
        imageArchive = ImageArchive.FromFileName(result.FullPath);
        var image = imageArchive.GetPage(0);
        Renderer.SetImage(image);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        Renderer.OnPanUpdated(this, e);
    }

    private void Renderer_TappedLeft(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        if (readingDirection == Direction.LeftToRight)
        {
            if (pageIndex == 0)
            {
                return;
            }
            pageIndex--;
        }
        if (readingDirection == Direction.RightToLeft)
        {
            if (pageIndex == imageArchive.PageCount - 1)
            {
                return;
            }
            pageIndex++;
        }
        var image = imageArchive.GetPage(pageIndex);
        Renderer.SetImage(image);
    }

    private void Renderer_TappedRight(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        if (readingDirection == Direction.RightToLeft)
        {
            if (pageIndex == 0)
            {
                return;
            }
            pageIndex--;
        }
        if (readingDirection == Direction.LeftToRight)
        {
            if (pageIndex == imageArchive.PageCount - 1)
            {
                return;
            }
            pageIndex++;
        }
        var image = imageArchive.GetPage(pageIndex);
        Renderer.SetImage(image);
    }

    private void Renderer_TappedCenter(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        if (Menu.IsVisible)
        {
            Menu.IsVisible = false;
        }
    }

    private void Renderer_TappedTop(object sender, TappedEventArgs e)
    {
        Menu.IsVisible = true;
    }
}

