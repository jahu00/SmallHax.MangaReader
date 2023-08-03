using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Handlers;
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
        private SKPoint panStartOffset;
        private PanUpdatedEventArgs panStartEvent;

        private float _zoom = 1f;
        private float pinchStartZoom = 1f;
        private PinchGestureUpdatedEventArgs pinchStartEvent;

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
            var tapGestureRecognizer = new TapGestureRecognizer { Buttons = ButtonsMask.Primary };
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
                var point = e.GetCurrentPoint(view);
                var wheelDelta = point.Properties.MouseWheelDelta;
                if (wheelDelta == 0)
                {
                    return;
                }
                if (wheelDelta > 0)
                {
                    ZoomIn();
                    return;
                }
                ZoomOut();
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
            if (e.Status == GestureStatus.Completed)
            {
                Zoom = (float)Math.Round(Zoom, 2);
                return;
            }
            if (e.Status == GestureStatus.Started)
            {
                pinchStartEvent = e;
                pinchStartZoom = Zoom;
            }
            Zoom = pinchStartZoom * (float)(e.Scale - pinchStartEvent.Scale);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            if (Handler == null || Parent == null || Image == null)
            {
                return;
            }

            var canvas = e.Surface.Canvas;
            //canvas.TotalMatrix
            canvas.Clear();
            var matrix = canvas.TotalMatrix;// SKMatrix.Identity;
            var zoomedWidth = Image.Width * Zoom;
            var zoomedHeight = Image.Height * Zoom;
            if (Zoom != 1)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateScale(Zoom, Zoom));
            }
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(offset.X, offset.Y));
            if (Width > zoomedWidth)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)Width / 2 - zoomedWidth / 2, 0));
            }
            else if (ReadingDirection == Direction.RightToLeft)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)Width - zoomedWidth, 0));
            }
            if (Height > zoomedHeight)
            {
                matrix = matrix.PostConcat(SKMatrix.CreateTranslation(0, (float)Height / 2 - zoomedHeight / 2));
            }
            canvas.SetMatrix(matrix);
            canvas.DrawImage(Image, 0, 0, paint);
        }

        private void SetImage(SKImage image)
        {
            _image = image;
            offset.X = 0;
            offset.Y = 0;
            if (_autoZoom)
            {
                FillZoom();
            }
            else
            {
                ResetZoom();
            }
        }

        public void ZoomIn()
        {
            Zoom += 0.1f;
            Zoom = (float)Math.Round(Zoom, 1);
        }

        public void ZoomOut()
        {
            Zoom -= 0.1f;
            Zoom = (float)Math.Round(Zoom, 1);
            if (Zoom < 0.1f)
            {
                Zoom = 0.1f;
            }
        }

        public void ResetZoom()
        {
            Zoom = 1;
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
                return;
            }
            Zoom = (float)Width / Image.Width;
        }
    }
}
