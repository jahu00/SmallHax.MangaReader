using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SmallHax.MangaReader.Models;
using System.ComponentModel;

namespace SmallHax.MangaReader.Controls
{
    public class ImageRenderer : SKCanvasView
    {
        private string[] UpdatablePropertyNames = { nameof(ReadingDirection), nameof(Image), nameof(Zoom) };

        private SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High };
        private SKImage _image;

        private SKPoint offset = new SKPoint(0, 0);
        private SKPoint actualOffset = new SKPoint(0, 0);
        private SKPoint panStartOffset;
        private PanUpdatedEventArgs panStartEvent;

        private float _zoom = 1f;

        private Direction _readingDirection;
        private bool _autoZoom;

        public float Center { get; set; } = 0.10f;
        public float Top { get; set; } = 0.25f;
        public float Bottom { get; set; } = 0.75f;

        public event EventHandler<TappedEventArgs> TappedRight;
        public event EventHandler<TappedEventArgs> TappedLeft;
        public event EventHandler<TappedEventArgs> TappedCenter;
        public event EventHandler<TappedEventArgs> TappedTop;
        public event EventHandler<TappedEventArgs> TappedBottom;

        public Direction ReadingDirection { get { return _readingDirection; } set { _readingDirection = value; base.OnPropertyChanged(); } }

        public float Zoom { get { return _zoom; } set { _zoom = value; base.OnPropertyChanged(); } }

        public SKImage Image { get { return _image; } set { SetImage(value); base.OnPropertyChanged(); } }

        public bool AutoZoom { get { return _autoZoom; } set { _autoZoom = value; base.OnPropertyChanged(); } }

        public ImageRenderer() : base()
        {
            PropertyChanged += OnPropertyChanged;
            var panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGestureRecognizer);
            var tapGestureRecognizer = new TapGestureRecognizer { Buttons = ButtonsMask.Primary, NumberOfTapsRequired = 1 };
            tapGestureRecognizer.Tapped += OnTapped;
            GestureRecognizers.Add(tapGestureRecognizer);
            var pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinchGestureRecognizer);
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if WINDOWS
              var view = Handler.PlatformView as SkiaSharp.Views.Windows.SKXamlCanvas;
              view.PointerWheelChanged += (s, e) =>
              {
                if (Image == null)
                {
                    return;
                }
                var point = e.GetCurrentPoint(view);
                var wheelDelta = point.Properties.MouseWheelDelta;
                var position = new SKPoint((float)point.Position.X, (float)point.Position.Y);
                if (wheelDelta == 0)
                {
                    return;
                }
                if (wheelDelta > 0)
                {
                    ZoomIn(position);
                    return;
                }
                ZoomOut(position);
            };
#endif
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
            if (Image == null)
            {
                return;
            }
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

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (Image == null)
            {
                return;
            }

            if (e.Status == GestureStatus.Started)
            {
                return;
            }
            var startZoom = Zoom;
            if (e.Status == GestureStatus.Completed)
            {
                Zoom = (float)Math.Round(Zoom, 2);
            }
            else
            {
                Zoom = Zoom * (float)e.Scale;
            }
            //Console.WriteLine($"{e.ScaleOrigin.X}, {(float)e.ScaleOrigin.Y}");
            PostZoomAdjustOffset(new SKPoint((float)(e.ScaleOrigin.X * Width), (float)(e.ScaleOrigin.Y * Height)), startZoom);

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
            if (Zoom != 1)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateScale(Zoom, Zoom));
            }
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(offset.X, offset.Y));
            var density = (float)DeviceDisplay.MainDisplayInfo.Density;
            if (density != 1)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateScale(density, density));
            }

            actualOffset.X = matrix.TransX;
            actualOffset.Y = matrix.TransY;
            canvas.SetMatrix(matrix);
            canvas.DrawImage(Image, 0, 0, paint);
        }

        private void SetImage(SKImage image)
        {
            _image = image;
            if (_autoZoom)
            {
                FillZoom();
            }
            else
            {
                ResetZoom();
            }
        }

        private void AutoOffset()
        {
            offset.X = 0;
            offset.Y = 0;
            var zoomedWidth = Image.Width * Zoom;
            var zoomedHeight = Image.Height * Zoom;
            if (Width > zoomedWidth)
            {
                offset.X += (float)Width / 2 - zoomedWidth / 2;
            }
            else if (ReadingDirection == Direction.RightToLeft)
            {
                offset.X += (float)Width - zoomedWidth;
            }
            if (Height > zoomedHeight)
            {
                offset.Y += (float)Height / 2 - zoomedHeight / 2;
            }
        }

        public void ZoomIn(SKPoint? origin = null)
        {
            if (origin == null)
            {
                origin = actualOffset;
            }
            var startZoom = Zoom;
            Zoom += 0.1f;
            Zoom = (float)Math.Round(Zoom, 1);
            PostZoomAdjustOffset(origin, startZoom);
        }

        private void PostZoomAdjustOffset(SKPoint? origin, float startZoom)
        {
            var zeroBasedOffset = origin.Value - actualOffset;
            var startZoomedWidth = Image.Width * startZoom;
            var startZoomedHeight = Image.Height * startZoom;
            var zoomedWidth = Image.Width * Zoom;
            var zoomedHeight = Image.Height * Zoom;
            var proportionalX = zeroBasedOffset.X / startZoomedWidth;
            var proportionalY = zeroBasedOffset.Y / startZoomedHeight;
            var newX = proportionalX * zoomedWidth;
            var newY = proportionalY * zoomedHeight;
            var offsetDeltaX = newX - zeroBasedOffset.X;
            var offsetDeltaY = newY - zeroBasedOffset.Y;
            offset.X -= offsetDeltaX;
            offset.Y -= offsetDeltaY;
        }

        public void ZoomOut(SKPoint? origin = null)
        {
            if (origin == null)
            {
                origin = actualOffset;
            }
            var startZoom = Zoom;
            Zoom -= 0.1f;
            Zoom = (float)Math.Round(Zoom, 1);
            if (Zoom < 0.1f)
            {
                Zoom = 0.1f;
            }
            PostZoomAdjustOffset(origin, startZoom);
        }

        public void ResetZoom()
        {
            Zoom = 1;
            AutoOffset();
        }

        public void FillZoom()
        {
            if (Image == null)
            {
                return;
            }
            var rendererProportions = Width / Height;
            var imageProportions = (double)Image.Width / (double)Image.Height;
            if (rendererProportions > imageProportions)
            {
                Zoom = (float)Height / Image.Height;
            }
            else
            {
                Zoom = (float)Width / Image.Width;
            }
            AutoOffset();
        }
    }
}
