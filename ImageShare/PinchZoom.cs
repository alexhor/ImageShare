using ImageShare.Views;

namespace Bertuzzi.MAUI.PinchZoomImage
{
    /// <summary>
    /// PinchZoom View from TBertuzzi:
    /// https://github.com/TBertuzzi/Bertuzzi.MAUI.PinchZoomImage
    /// </summary>
    public class PinchZoom : ContentView
    {
        private double _currentScale = 1;
        private double _startScale = 1;
        private double _xOffset = 0;
        private double _yOffset = 0;
        private bool _secondDoubleTapp = false;

        private FullscreenImage fullscreenImageView;

        public PinchZoom(FullscreenImage fullscreenImage)
        {
            fullscreenImageView = fullscreenImage;

            var pinchGesture = new PinchGestureRecognizer();
            pinchGesture.PinchUpdated += PinchUpdated;
            GestureRecognizers.Add(pinchGesture);

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);

            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tapGesture.Tapped += DoubleTapped;
            GestureRecognizers.Add(tapGesture);
        }

        private void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _startScale = Content.Scale;
                    Content.AnchorX = 0;
                    Content.AnchorY = 0;
                    break;
                case GestureStatus.Running:
                    {
                        _currentScale += (e.Scale - 1) * _startScale;
                        _currentScale = Math.Max(1, _currentScale);

                        var renderedX = Content.X + _xOffset;
                        var deltaX = renderedX / Width;
                        var deltaWidth = Width / (Content.Width * _startScale);
                        var originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                        var renderedY = Content.Y + _yOffset;
                        var deltaY = renderedY / Height;
                        var deltaHeight = Height / (Content.Height * _startScale);
                        var originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                        var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                        var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                        Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                        Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                        Content.Scale = _currentScale;
                        break;
                    }
                case GestureStatus.Completed:
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void NextImage()
        {
            if (LoadingDifferentImage)
                return;
            LoadingDifferentImage = fullscreenImageView.NextImage();
        }
        private bool LoadingDifferentImage = false;
        private bool LeftSideTouched = false;
        private DateTime LeftSideTouchedSince = DateTime.MinValue;
        private TimeSpan SideTouchedTimeout = new TimeSpan(2000000);

        /// <summary>
        /// 
        /// </summary>
        protected void PreviousImage()
        {
            if (LoadingDifferentImage)
                return;
            LoadingDifferentImage = fullscreenImageView.PreviousImage();

        }
        private bool RightSideTouched = false;
        private DateTime RightSideTouchedSince = DateTime.MinValue;

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale == 1)
            {
                // Next image
                if (0 > e.TotalX)
                {
                    NextImage();
                }
                // Previous image
                else if (0 < e.TotalX)
                {
                    PreviousImage();
                }
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Running:

                    var newX = (e.TotalX * Scale) + _xOffset;
                    var newY = (e.TotalY * Scale) + _yOffset;

                    var width = (Content.Width * Content.Scale);
                    var height = (Content.Height * Content.Scale);

                    var canMoveX = width > Application.Current.MainPage.Width;
                    var canMoveY = height > Application.Current.MainPage.Height;

                    if (canMoveX)
                    {
                        var minX = (width - (Application.Current.MainPage.Width / 2)) * -1;
                        var maxX = Math.Min(Application.Current.MainPage.Width / 2, width / 2);

                        if (newX <= minX)
                        {
                            // Next image
                            if (RightSideTouched && DateTime.Now > RightSideTouchedSince.Add(SideTouchedTimeout))
                            {
                                NextImage();
                                return;
                            }

                            newX = minX;
                            RightSideTouched = true;
                            RightSideTouchedSince = DateTime.Now;
                        }
                        else
                        {
                            RightSideTouched = false;
                        }

                        if (newX >= maxX)
                        {
                            // Next image
                            if (LeftSideTouched && DateTime.Now > LeftSideTouchedSince.Add(SideTouchedTimeout))
                            {
                                PreviousImage();
                                return;
                            }

                            newX = maxX;
                            LeftSideTouched = true;
                            LeftSideTouchedSince = DateTime.Now;
                        }
                        else
                        {
                            LeftSideTouched = false;
                        }
                    }
                    else
                    {
                        newX = 0;
                    }

                    if (canMoveY)
                    {
                        var minY = (height - (Application.Current.MainPage.Height / 2)) * -1;
                        var maxY = Math.Min(Application.Current.MainPage.Width / 2, height / 2);

                        if (newY < minY)
                        {
                            newY = minY;
                        }

                        if (newY > maxY)
                        {
                            newY = maxY;
                        }
                    }
                    else
                    {
                        newY = 0;
                    }

                    Content.TranslationX = newX;
                    Content.TranslationY = newY;
                    break;
                case GestureStatus.Completed:
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
                case GestureStatus.Started:
                    break;
                case GestureStatus.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async void DoubleTapped(object sender, TappedEventArgs e)
        {
            var multiplicator = Math.Pow(2, 1.0 / 10.0);
            _startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            for (var i = 0; i < 10; i++)
            {
                if (!_secondDoubleTapp) //if it's not the second double tapp we enlarge the scale
                {
                    _currentScale *= multiplicator;
                }
                else //if it's the second double tap we make the scale smaller again 
                {
                    _currentScale /= multiplicator;
                    _currentScale = 1;
                }

                var renderedX = Content.X + _xOffset;
                var deltaX = renderedX / Width;
                var deltaWidth = Width / (Content.Width * _startScale);
                var originX = (0.5 - deltaX) * deltaWidth;

                var renderedY = Content.Y + _yOffset;
                var deltaY = renderedY / Height;
                var deltaHeight = Height / (Content.Height * _startScale);
                var originY = (0.5 - deltaY) * deltaHeight;

                var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                Content.Scale = _currentScale;
                await Task.Delay(10);
            }
            _secondDoubleTapp = !_secondDoubleTapp;
            _xOffset = Content.TranslationX;
            _yOffset = Content.TranslationY;
        }
    }
}
