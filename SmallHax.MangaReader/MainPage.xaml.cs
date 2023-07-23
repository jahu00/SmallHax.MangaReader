using SmallHax.MangaReader.Models;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace SmallHax.MangaReader;

public partial class MainPage : ContentPage
{
    private ImageArchive imageArchive;

    private Direction _readingDirection;
    public Direction ReadingDirection { get { return _readingDirection; } set { SetReadingDirection(value); base.OnPropertyChanged(); } }

    private FilePickerFileType fileTypes = new FilePickerFileType(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".cbz", ".zip" } }, // file extension
        }
    );

    private void SetReadingDirection(Direction value)
    {
        _readingDirection = value;
        DirectionMenuItem.Icon = value == Direction.LeftToRight ? FontAwesome.ArrowRight : FontAwesome.ArrowLeft;
        Renderer.ReadingDirection = value;
        Preferences.Set(nameof(ReadingDirection), value.ToString());
    }

    private int pageIndex = 0;

    public MainPage()
	{
		InitializeComponent();
        var storedLastReadingDirection = Preferences.Get(nameof(ReadingDirection), Direction.LeftToRight.ToString());
        if (Enum.TryParse<Direction>(storedLastReadingDirection, out var lastReadingDirection))
        {
            ReadingDirection = lastReadingDirection;
        }

    }

    private async void Open_Tapped(object sender, TappedEventArgs e)
    {
        var pickOptions = new PickOptions
        {
            FileTypes = fileTypes
        };
        var result = await FilePicker.PickAsync(pickOptions);
        if (result == null)
        {
            return;
        }
        try
        {
            imageArchive = ImageArchive.FromFileName(result.FullPath);
            pageIndex = 0;
            UpdatePage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"There was an error oppening file: {result.FullPath}", "OK");
        }
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
        var image = imageArchive.GetPage(pageIndex, out var pageFileName);
        Renderer.Image = image;
        PageFileNameLabel.Text = pageFileName;
        PageCountLabel.Text = $"{pageIndex + 1} / {imageArchive.PageCount}";
    }

    private void Renderer_TappedCenter(object sender, TappedEventArgs e)
    {
        Progress.IsVisible = false;
        Menu.IsVisible = false;
    }

    private void Renderer_TappedTop(object sender, TappedEventArgs e)
    {
        Menu.IsVisible = true;
    }

    private void Renderer_TappedBottom(object sender, TappedEventArgs e)
    {
        Progress.IsVisible = true;
    }
}

