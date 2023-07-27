﻿using SmallHax.MangaReader.Models;

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
        var filePckerResult = await FilePicker.PickAsync(pickOptions);
        if (filePckerResult == null)
        {
            return;
        }
        try
        {
            imageArchive = ImageArchive.FromFileName(filePckerResult.FullPath);
            GoToPage(0);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"There was an error oppening file: {filePckerResult.FullPath}", "OK");
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
        GoToPage(pageIndex + 1);
    }

    private void PreviousPage()
    {
        if (pageIndex == 0)
        {
            return;
        }
        GoToPage(pageIndex - 1);
    }

    private async void GoToPage(int i)
    {
        if (i < 0 || i >= imageArchive.PageCount)
        {
            await DisplayAlert("Error", $"Invalid page number: {i + 1}", "OK");
            return;
        }
        pageIndex = i;
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

    private async void Progress_Tapped(object sender, TappedEventArgs e)
    {
        if (imageArchive == null)
        {
            return;
        }
        var promptResult = await DisplayPromptAsync("Go to page", $"Select 1-{imageArchive.PageCount}", initialValue: (pageIndex + 1).ToString(), keyboard: Keyboard.Numeric);
        // Detect if cancel was pressed
        if (promptResult == null)
        {
            return;
        }
        if (!int.TryParse(promptResult, out var newPageNumber))
        {
            await DisplayAlert("Error", $"Page number was not in correct format: {promptResult}", "OK");
            return;
        }
        GoToPage(newPageNumber - 1);
    }

    private void ZoomIn_Tapped(object sender, TappedEventArgs e)
    {
        Renderer.ZoomIn();
    }

    private void ZoomOut_Tapped(object sender, TappedEventArgs e)
    {
        Renderer.ZoomOut();
    }

    private void ResetZoom_Tapped(object sender, TappedEventArgs e)
    {
        Renderer.ResetZoom();
    }

    private void FillZoom_Tapped(object sender, TappedEventArgs e)
    {
        Renderer.FillZoom();
    }
}

