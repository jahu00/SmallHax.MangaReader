using Microsoft.Maui;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SmallHax.MangaReader.Services;
using SmallHax.Utils;
using System.ComponentModel;

namespace SmallHax.MangaReader.Controls
{
    public class SKLabel : SKCanvasView
    {
        private string[] UpdatablePropertyNames = { nameof(Text), nameof(Width), nameof(Height), nameof(FontSize), nameof(FontFamily), nameof(Parent), nameof(Color), nameof(Parent), nameof(Handler) };
        private string[] PaintPropertyNames = { nameof(FontSize), nameof(FontFamily), nameof(Color), nameof(Width), nameof(Height), nameof(Handler) };
        private ISKFontManager _fontManager;
        private double _fontSize;
        private string _fontFamily;
        private Color _color = Colors.Black;
        private SKPaint _paint;

        //private readonly TapGestureRecognizer tapGestureRecognizer;

        private string _text { get; set; }
        public string Text { get { return _text; } set { _text = value; base.OnPropertyChanged(); } }
        public double FontSize { get { return _fontSize; } set { _fontSize = value; base.OnPropertyChanged(); } }
        public string FontFamily { get { return _fontFamily; } set { _fontFamily = value; base.OnPropertyChanged(); } }
        public Color Color { get { return _color;  } set { _color = value; base.OnPropertyChanged(); } }

        public SKLabel() : base()
        {
            PropertyChanged += OnPropertyChanged;
            //tapGestureRecognizer = new TapGestureRecognizer { Buttons = ButtonsMask.Primary };
            //tapGestureRecognizer.Tapped += OnTapped;
            //GestureRecognizers.Add(tapGestureRecognizer);
        }

        /*private void OnTapped(object sender, TappedEventArgs e)
        {
            var position = e.GetPosition(this).Value;
            var character = Characters.FirstOrDefault(x => x.Rect.Contains((float)position.X, (float)position.Y));
            TextTapped?.Invoke(this, character?.Index);
        }*/

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            _fontManager = Handler.MauiContext.Services.GetService<ISKFontManager>();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Handler == null)
            {
                return;
            }
            if (PaintPropertyNames.Contains(e.PropertyName))
            {
                var font = _fontManager.Get($"{FontFamily}.ttf");
                _paint = new SKPaint(font) { ColorF = new SKColorF(_color.Red, _color.Green, _color.Blue, _color.Alpha), TextSize = (float)_fontSize };
                WidthRequest = _paint.MeasureText(Text);
                HeightRequest = FontSize;
            }
            if (UpdatablePropertyNames.Contains(e.PropertyName))
            {
                InvalidateSurface();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            if (Handler == null || Parent == null)
            {
                return;
            }

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            canvas.DrawText(Text, 0, (float)FontSize, _paint);
        }
    }
}
