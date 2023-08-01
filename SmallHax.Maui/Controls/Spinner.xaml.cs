using System.ComponentModel;

namespace SmallHax.Maui.Controls;

public partial class Spinner : ContentView
{
    public Spinner()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsVisible")
        {
            ActivityIndicator.IsRunning = IsVisible;
        }
    }
}