using SmallHax.MangaReader.Models;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace SmallHax.MangaReader;

public partial class MainPage : ContentPage
{
    private ImageArchive imageArchive;

    private Direction _readingDirection;
    public Direction ReadingDirection { get { return _readingDirection; } set { SetReadingDirection(value); base.OnPropertyChanged(); } }

    private void SetReadingDirection(Direction value)
    {
        _readingDirection = value;
        DirectionMenuItem.Icon = value == Direction.LeftToRight ? FontAwesome.ArrowRight : FontAwesome.ArrowLeft;
        Renderer.ReadingDirection = value;
    }

    private int pageIndex = 0;

    public MainPage()
	{
		InitializeComponent();
        ReadingDirection = Direction.LeftToRight;

    }

    private async void Open_Tapped(object sender, TappedEventArgs e)
    {
        var result = await FilePicker.PickAsync(PickOptions.Default);
        if (result == null)
        {
            return;
        }
        imageArchive = ImageArchive.FromFileName(result.FullPath);
        pageIndex = 0;
        UpdatePage();
    }

    private void Direction_Tapped(object sender, TappedEventArgs e)
    {
        if (ReadingDirection == Direction.LeftToRight)
        {
            ReadingDirection = Direction.RightToLeft;
        }
        else
        {
            ReadingDirection = Direction.LeftToRight;
        }
    }

    private void Renderer_TappedLeft(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        if (ReadingDirection == Direction.LeftToRight)
        {
            PreviousPage();
        }
        if (ReadingDirection == Direction.RightToLeft)
        {
            NextPage();
        }
    }

    private void Renderer_TappedRight(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        if (ReadingDirection == Direction.RightToLeft)
        {
            PreviousPage();
        }
        if (ReadingDirection == Direction.LeftToRight)
        {
            NextPage();
        }
    }

    private void NextPage()
    {
        if (pageIndex == imageArchive.PageCount - 1)
        {
            return;
        }
        pageIndex++;
        UpdatePage();
    }

    private void PreviousPage()
    {
        if (pageIndex == 0)
        {
            return;
        }
        pageIndex--;
        UpdatePage();
    }

    private void UpdatePage()
    {
        var image = imageArchive.GetPage(pageIndex);
        Renderer.Image = image;
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

