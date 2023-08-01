using SmallHax.MangaReader.Models;
using SmallHax.Maui.Helpers;

namespace SmallHax.MangaReader;

public partial class MainPage : ContentPage
{
    private FilePickerFileType fileTypes = new FilePickerFileType(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".cbz", ".zip" } }, // file extension
        }
    );

    private ImageArchive imageArchive;

    private Direction _readingDirection;
    public Direction ReadingDirection { get { return _readingDirection; } set { SetReadingDirection(value); base.OnPropertyChanged(); } }

    private bool _autoZoom;
    public bool AutoZoom { get { return _autoZoom; } set { SetAutoZoom(value); base.OnPropertyChanged(); } }

    private bool _restore;
    public bool Restore { get { return _restore; } set { SetRestore(value); base.OnPropertyChanged(); } }

    private string LastFileName { get; set; }
    private int LastPageIndex { get; set; }

    private void SetAutoZoom(bool value)
    {
        _autoZoom = value;
        AutoZoomMenuItem.Icon = value ? FontAwesome.SquareCheck : FontAwesome.Square;
        Renderer.AutoZoom = value;
        Preferences.Set(nameof(AutoZoom), value.ToString());
    }

    private void SetRestore(bool value)
    {
        _restore = value;
        RestoreMenuItem.Icon = value ? FontAwesome.SquareCheck : FontAwesome.Square;
        Preferences.Set(nameof(Restore), value.ToString());
    }

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
        ReadingDirection = PreferencesHelper.GetEnum(nameof(ReadingDirection), Direction.LeftToRight);
        AutoZoom = Preferences.Get(nameof(AutoZoom), false);
        Restore = Preferences.Get(nameof(Restore), true);
        Restore = Preferences.Get(nameof(Restore), true);
        LastFileName = Preferences.Get(nameof(LastFileName), null);
        LastPageIndex = Preferences.Get(nameof(LastPageIndex), 0);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (Restore && File.Exists(LastFileName))
        {
            await TryOpen(LastFileName, LastPageIndex);
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
        await TryOpen(filePckerResult.FullPath);
    }

    private async Task TryOpen(string path, int newPageIndex = 0)
    {
        try
        {
            Spinner.IsVisible = true;
            imageArchive = ImageArchive.FromFileName(path);
            Preferences.Set(nameof(LastFileName), path);
            GoToPage(newPageIndex);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"There was an error oppening file: {path}", "OK");
        }
        finally
        {
            Spinner.IsVisible = false;
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
        Preferences.Set(nameof(LastPageIndex), pageIndex);
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

    private void AutoZoom_Tapped(object sender, TappedEventArgs e)
    {
        AutoZoom = !AutoZoom;
    }

    private void Restore_Tapped(object sender, TappedEventArgs e)
    {
        Restore = !Restore;
    }

    private void ContentPage_SizeChanged(object sender, EventArgs e)
    {
        if (!AutoZoom)
        {
            return;
        }
        Renderer.FillZoom();
    }
}

