namespace SmallHax.MangaReader.Controls;

public partial class ManuItem : ContentView
{
    public string Text { get { return Label.Text; } set { Label.Text = value; base.OnPropertyChanged(); } }
    public string Icon { get { return IconLabel.Text; } set { IconLabel.Text = value; base.OnPropertyChanged(); } }
    public string IconFontFamily { get { return IconLabel.FontFamily; } set { IconLabel.FontFamily = value; base.OnPropertyChanged(); } }

    public event EventHandler<TappedEventArgs> Tapped;

    public ManuItem()
	{
		InitializeComponent();
	}

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Tapped?.Invoke(this, e);
    }
}