using System.ComponentModel;

namespace SmallHax.MangaReader.Controls;

// TODO: This is unused and is prime candidate for removal

public partial class TapArea : ContentView
{
    private string _text;

    public event EventHandler<TappedEventArgs>? Tapped;
    public event EventHandler<PanUpdatedEventArgs>? PanUpdated;
    public event EventHandler<PinchGestureUpdatedEventArgs>? PinchUpdated;

    public string Text { get { return Label.Text; } set { Label.Text = value; base.OnPropertyChanged(); } }
    public bool IsLabelVisible { get { return Label.IsVisible; } set { Label.IsVisible = value; base.OnPropertyChanged(); } }

    public TapArea()
	{
		InitializeComponent();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Tapped?.Invoke(this, e);
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        PanUpdated?.Invoke(this, e);
    }

    private void PinchGestureRecognizer_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        PinchUpdated?.Invoke(this, e);
    }
}