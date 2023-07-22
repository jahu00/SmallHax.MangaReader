using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SmallHax.MangaReader.Models;
using System.ComponentModel;

namespace SmallHax.MangaReader.Controls
{
    public class ImageRenderer : SKCanvasView
    {
        private string[] UpdatablePropertyNames = { nameof(ReadingDirection), nameof(Image) };

        private SKImage _image;

        private SKPoint offset = new SKPoint(0, 0);
        private SKPoint panStartOffset;
        private PanUpdatedEventArgs panStartEvent;
        private Direction _readingDirection;

        public float Center { get; set; } = 0.10f;
        public float Top { get; set; } = 0.25f;
        public float Bottom { get; set; } = 0.75f;

        public event EventHandler<TappedEventArgs> TappedRight;
        public event EventHandler<TappedEventArgs> TappedLeft;
        public event EventHandler<TappedEventArgs> TappedCenter;
        public event EventHandler<TappedEventArgs> TappedTop;
        public event EventHandler<TappedEventArgs> TappedBottom;

        public Direction ReadingDirection { get { return _readingDirection; } set { _readingDirection = value; base.OnPropertyChanged(); } }
        public SKImage Image { get { return _image; } set { SetImage(value); base.OnPropertyChanged(); } }

        public ImageRenderer() : base()
        {
            PropertyChanged += OnPropertyChanged;
            var panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGestureRecognizer);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnTapped;
            GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (UpdatablePropertyNames.Contains(e.PropertyName))
            {
                InvalidateSurface();
            }
        }

        private void OnTapped(object sender, TappedEventArgs e)
        {
            var position = e.GetPosition(this).Value;
            var proportionalY = position.Y / Height;
            if (proportionalY < Top)
            {
                TappedTop?.Invoke(this, e);
                return;
            }
            if (proportionalY > Bottom)
            {
                TappedBottom?.Invoke(this, e);
                return;
            }
            var proportionalX = position.X / Width;
            if (proportionalX > 0.5f - Center && proportionalX < 0.5f + Center)
            {
                TappedCenter?.Invoke(this, e);
                return;
            }
            if (proportionalX < 0.5f)
            {
                TappedLeft?.Invoke(this, e);
                return;
            }
            TappedRight?.Invoke(this, e);
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Completed)
            {
                InvalidateSurface();
                return;
            }
            if (e.StatusType == GestureStatus.Started)
            {
                panStartEvent = e;
                panStartOffset = offset;
            }
            offset.X = panStartOffset.X + (float)(e.TotalX + panStartEvent.TotalX);
            offset.Y = panStartOffset.Y + (float)(e.TotalY + panStartEvent.TotalY);
            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            if (Handler == null || Parent == null || Image == null)
            {
                return;
            }

            var canvas = e.Surface.Canvas;
            canvas.Clear();
            var matrix = SKMatrix.Identity;
            if (Width > Image.Width)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)Width / 2 - Image.Width / 2, 0));
            }
            else if (ReadingDirection == Direction.RightToLeft)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)Width - Image.Width, 0));
            }
            if (Height > _image.Height)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation(0, (float)Height / 2 - _image.Height / 2));
            }
            canvas.SetMatrix(matrix);
            canvas.DrawImage(Image, offset);
        }

        private void SetImage(SKImage image)
        {
            _image = image;
            offset.X = 0;
            offset.Y = 0;
        }
    }
}
