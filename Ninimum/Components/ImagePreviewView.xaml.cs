using Microsoft.Maui.Controls;

namespace Ninimum.Components;

public partial class ImagePreviewView : ContentView
{
    private const double MinScale = 1.0;
    private const double MaxScale = 4.0;
    private const double DoubleTapScale = 2.5;
    private const double ZoomEpsilon = 0.015;
    private const double OverlayOpacity = 0.55;
    private const double DismissDistance = 120.0;

    private bool _isAnimating;
    private bool _isPinching;
    private bool _dismissGestureActive;
    private double _currentScale = MinScale;
    private double _translationX;
    private double _translationY;
    private double _dismissStartY;
    private double _pinchLastFocusX;
    private double _pinchLastFocusY;
    private bool _pinchFocusReady;

#if ANDROID
    private Android.Views.View? _androidGestureView;
    private Android.Views.ScaleGestureDetector? _androidScaleDetector;
    private Android.Views.GestureDetector? _androidGestureDetector;
    private double _androidDensity = 1.0;
    private double _androidLastTouchX;
    private double _androidLastTouchY;
    private bool _androidSingleTouchReady;
#endif

#if IOS
    private double _iosLastPinchScale = 1.0;
    private double _iosLastPanX;
    private double _iosLastPanY;
    private bool _iosPanStartedZoomed;
#endif

    public ImagePreviewView()
    {
        InitializeComponent();
        gestureSurface.HandlerChanged += OnGestureSurfaceHandlerChanged;
#if IOS
        SetupIosGestures();
#endif
    }

    public static readonly BindableProperty PreviewImageSourceProperty =
        BindableProperty.Create(
            nameof(PreviewImageSource),
            typeof(ImageSource),
            typeof(ImagePreviewView),
            default(ImageSource));

    public ImageSource PreviewImageSource
    {
        get => (ImageSource)GetValue(PreviewImageSourceProperty);
        set => SetValue(PreviewImageSourceProperty, value);
    }

    public async Task ShowAsync(ImageSource imageSource)
    {
        if (_isAnimating || imageSource == null)
            return;

        _isAnimating = true;
        PreviewImageSource = imageSource;
        ResetTransform();

        InputTransparent = false;
        boxFullImage.IsVisible = true;
        boxFullImage.Opacity = 0;

        fullImage.IsVisible = true;
        fullImage.Opacity = 0;
        fullImage.Scale = 0.97;

        await Task.WhenAll(
            boxFullImage.FadeTo(OverlayOpacity, 160, Easing.CubicOut),
            fullImage.FadeTo(1, 160, Easing.CubicOut),
            fullImage.ScaleTo(MinScale, 180, Easing.CubicOut));

        ApplyTransform(MinScale, 0, 0);
        _isAnimating = false;
    }

    public async Task CloseAsync(bool swipeDown = true)
    {
        if (_isAnimating || !fullImage.IsVisible)
            return;

        _isAnimating = true;
        _isPinching = false;
        _dismissGestureActive = false;

        double viewportHeight = GetViewportHeight();
        double targetY = (swipeDown ? 1 : -1) * Math.Max(320, viewportHeight * 0.75);

        await Task.WhenAll(
            fullImage.TranslateTo(0, targetY, 180, Easing.CubicIn),
            fullImage.ScaleTo(0.94, 180, Easing.CubicIn),
            fullImage.FadeTo(0, 150, Easing.CubicIn),
            boxFullImage.FadeTo(0, 180, Easing.CubicIn));

        FinishClosing();
        _isAnimating = false;
    }

    private bool IsZoomed => _currentScale > MinScale + ZoomEpsilon;

    private void OnGestureSurfaceHandlerChanged(object? sender, EventArgs e)
    {
#if ANDROID
        DetachAndroidGestures();

        if (gestureSurface.Handler?.PlatformView is Android.Views.View androidView)
            AttachAndroidGestures(androidView);
#endif

    }

    private void BeginPinch(double focusX, double focusY)
    {
        if (_isAnimating)
            return;

        _isPinching = true;
        _dismissGestureActive = false;
        _pinchLastFocusX = focusX;
        _pinchLastFocusY = focusY;
        _pinchFocusReady = true;

        // If the second finger arrives while a normal-scale dismiss drag has
        // already started, return to the stable 1x position before zooming.
        if (!IsZoomed)
        {
            ApplyTransform(MinScale, 0, 0);
            boxFullImage.Opacity = OverlayOpacity;
        }
    }

    private void UpdatePinch(double scaleFactor, double focusX, double focusY)
    {
        if (_isAnimating || !_isPinching || scaleFactor <= 0)
            return;

        if (!_pinchFocusReady)
        {
            _pinchLastFocusX = focusX;
            _pinchLastFocusY = focusY;
            _pinchFocusReady = true;
        }

        double oldScale = _currentScale;
        double newScale = Math.Clamp(oldScale * scaleFactor, MinScale, MaxScale);
        double ratio = oldScale <= 0 ? 1.0 : newScale / oldScale;

        // Keep the exact content point that was under the previous two-finger
        // midpoint under the new midpoint. This handles both zooming and the
        // natural movement of the two fingers, like Telegram/Photos.
        double targetX = focusX - (ratio * (_pinchLastFocusX - _translationX));
        double targetY = focusY - (ratio * (_pinchLastFocusY - _translationY));
        (targetX, targetY) = ClampTranslation(targetX, targetY, newScale);

        ApplyTransform(newScale, targetX, targetY);
        _pinchLastFocusX = focusX;
        _pinchLastFocusY = focusY;
    }

    private void EndPinch()
    {
        _isPinching = false;
        _pinchFocusReady = false;

        if (_currentScale <= MinScale + ZoomEpsilon)
        {
            ApplyTransform(MinScale, 0, 0);
            return;
        }

        var (x, y) = ClampTranslation(_translationX, _translationY, _currentScale);
        ApplyTransform(_currentScale, x, y);
    }

    private void PanZoomed(double deltaX, double deltaY)
    {
        if (_isAnimating || _isPinching || !IsZoomed)
            return;

        double targetX = _translationX + deltaX;
        double targetY = _translationY + deltaY;
        (targetX, targetY) = ClampTranslation(targetX, targetY, _currentScale);
        ApplyTransform(_currentScale, targetX, targetY);
    }

    private void BeginDismiss(double y)
    {
        if (_isAnimating || _isPinching || IsZoomed)
            return;

        _dismissGestureActive = true;
        _dismissStartY = y;
    }

    private void UpdateDismiss(double y)
    {
        if (_isAnimating || !_dismissGestureActive || _isPinching || IsZoomed)
            return;

        double deltaY = y - _dismissStartY;
        double viewportHeight = GetViewportHeight();
        double progress = Math.Clamp(Math.Abs(deltaY) / Math.Max(1, viewportHeight * 0.55), 0, 1);

        // Telegram-like drag-to-dismiss: the normal 1x image follows the
        // finger and becomes only slightly smaller while the background fades.
        fullImage.TranslationX = 0;
        fullImage.TranslationY = deltaY;
        fullImage.Scale = 1.0 - (0.06 * progress);
        boxFullImage.Opacity = OverlayOpacity * (1.0 - (0.82 * progress));
    }

    private async Task EndDismissAsync(double y)
    {
        if (!_dismissGestureActive)
            return;

        _dismissGestureActive = false;

        if (_isAnimating || IsZoomed)
            return;

        double deltaY = y - _dismissStartY;
        if (Math.Abs(deltaY) >= DismissDistance)
        {
            await CloseAsync(deltaY > 0);
            return;
        }

        // A simple tap should not start a restore animation. Otherwise the
        // first tap can block the second tap and make double-tap zoom unreliable.
        if (Math.Abs(deltaY) < 3)
        {
            ApplyTransform(MinScale, 0, 0);
            boxFullImage.Opacity = OverlayOpacity;
            return;
        }

        await RestoreNormalPositionAsync();
    }

    private async Task RestoreNormalPositionAsync()
    {
        if (_isAnimating || !fullImage.IsVisible)
            return;

        _isAnimating = true;

        await Task.WhenAll(
            fullImage.TranslateTo(0, 0, 150, Easing.CubicOut),
            fullImage.ScaleTo(MinScale, 150, Easing.CubicOut),
            boxFullImage.FadeTo(OverlayOpacity, 150, Easing.CubicOut));

        ApplyTransform(MinScale, 0, 0);
        _isAnimating = false;
    }

    private async Task ToggleDoubleTapZoomAsync(double focusX, double focusY)
    {
        if (_isAnimating || _isPinching || !fullImage.IsVisible)
            return;

        _isAnimating = true;
        _dismissGestureActive = false;

        double targetScale;
        double targetX;
        double targetY;

        if (IsZoomed)
        {
            targetScale = MinScale;
            targetX = 0;
            targetY = 0;
        }
        else
        {
            targetScale = DoubleTapScale;
            double viewportWidth = GetViewportWidth();
            double viewportHeight = GetViewportHeight();
            double centeredFocusX = focusX - (viewportWidth / 2.0);
            double centeredFocusY = focusY - (viewportHeight / 2.0);

            // Start from 1x/0 translation and zoom around the exact tapped point.
            targetX = centeredFocusX - (targetScale * centeredFocusX);
            targetY = centeredFocusY - (targetScale * centeredFocusY);
            (targetX, targetY) = ClampTranslation(targetX, targetY, targetScale);
        }

        await Task.WhenAll(
            fullImage.ScaleTo(targetScale, 190, Easing.CubicOut),
            fullImage.TranslateTo(targetX, targetY, 190, Easing.CubicOut),
            boxFullImage.FadeTo(OverlayOpacity, 120, Easing.CubicOut));

        ApplyTransform(targetScale, targetX, targetY);
        _isAnimating = false;
    }

    private (double X, double Y) ClampTranslation(double x, double y, double scale)
    {
        if (scale <= MinScale + ZoomEpsilon)
            return (0, 0);

        double viewportWidth = GetViewportWidth();
        double viewportHeight = GetViewportHeight();
        var (baseImageWidth, baseImageHeight) = GetBaseDisplayedImageSize(viewportWidth, viewportHeight);

        double maxX = Math.Max(0, ((baseImageWidth * scale) - viewportWidth) / 2.0);
        double maxY = Math.Max(0, ((baseImageHeight * scale) - viewportHeight) / 2.0);

        return (
            Math.Clamp(x, -maxX, maxX),
            Math.Clamp(y, -maxY, maxY));
    }

    private (double Width, double Height) GetBaseDisplayedImageSize(double viewportWidth, double viewportHeight)
    {
        double imageRatio = 0;

#if ANDROID
        if (fullImage.Handler?.PlatformView is Android.Widget.ImageView androidImage &&
            androidImage.Drawable != null &&
            androidImage.Drawable.IntrinsicWidth > 0 &&
            androidImage.Drawable.IntrinsicHeight > 0)
        {
            imageRatio = (double)androidImage.Drawable.IntrinsicWidth / androidImage.Drawable.IntrinsicHeight;
        }
#endif

#if IOS
        if (fullImage.Handler?.PlatformView is UIKit.UIImageView iosImage &&
            iosImage.Image != null &&
            iosImage.Image.Size.Width > 0 &&
            iosImage.Image.Size.Height > 0)
        {
            imageRatio = iosImage.Image.Size.Width / iosImage.Image.Size.Height;
        }
#endif

        if (imageRatio <= 0 || viewportWidth <= 0 || viewportHeight <= 0)
            return (viewportWidth, viewportHeight);

        double viewportRatio = viewportWidth / viewportHeight;
        if (imageRatio >= viewportRatio)
            return (viewportWidth, viewportWidth / imageRatio);

        return (viewportHeight * imageRatio, viewportHeight);
    }

    private double GetViewportWidth()
    {
        if (gestureSurface.Width > 0)
            return gestureSurface.Width;

        if (Width > 0)
            return Width;

        return Math.Max(1, fullImage.Width);
    }

    private double GetViewportHeight()
    {
        if (gestureSurface.Height > 0)
            return gestureSurface.Height;

        if (Height > 0)
            return Height;

        return Math.Max(1, fullImage.Height);
    }

    private void ApplyTransform(double scale, double translationX, double translationY)
    {
        _currentScale = Math.Clamp(scale, MinScale, MaxScale);
        _translationX = translationX;
        _translationY = translationY;

        fullImage.Scale = _currentScale;
        fullImage.TranslationX = _translationX;
        fullImage.TranslationY = _translationY;
    }

    private void ResetTransform()
    {
        _isPinching = false;
        _dismissGestureActive = false;
        _dismissStartY = 0;
        _pinchLastFocusX = 0;
        _pinchLastFocusY = 0;
        _pinchFocusReady = false;
        ApplyTransform(MinScale, 0, 0);
    }

    private void FinishClosing()
    {
        boxFullImage.IsVisible = false;
        boxFullImage.Opacity = 0;

        fullImage.IsVisible = false;
        fullImage.Opacity = 1;
        ResetTransform();

        InputTransparent = true;
    }

#if ANDROID
    private void AttachAndroidGestures(Android.Views.View view)
    {
        _androidGestureView = view;
        view.Clickable = true;
        _androidDensity = Math.Max(1.0, view.Context.Resources?.DisplayMetrics?.Density ?? 1.0f);
        _androidScaleDetector = new Android.Views.ScaleGestureDetector(view.Context, new AndroidScaleListener(this));
        _androidGestureDetector = new Android.Views.GestureDetector(view.Context, new AndroidGestureListener(this));
        view.Touch += OnAndroidTouch;
    }

    private void DetachAndroidGestures()
    {
        if (_androidGestureView != null)
            _androidGestureView.Touch -= OnAndroidTouch;

        _androidGestureView = null;
        _androidScaleDetector = null;
        _androidGestureDetector = null;
        _androidDensity = 1.0;
        _androidLastTouchX = 0;
        _androidLastTouchY = 0;
        _androidSingleTouchReady = false;
    }

    private double AndroidPxToDip(double pixels)
    {
        return pixels / Math.Max(1.0, _androidDensity);
    }

    private void OnAndroidTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if (!fullImage.IsVisible || _isAnimating)
        {
            e.Handled = fullImage.IsVisible;
            return;
        }

        Android.Views.MotionEvent motion = e.Event;
        _androidScaleDetector?.OnTouchEvent(motion);
        _androidGestureDetector?.OnTouchEvent(motion);

        switch (motion.ActionMasked)
        {
            case Android.Views.MotionEventActions.Down:
            {
                _androidGestureView?.Parent?.RequestDisallowInterceptTouchEvent(true);

                double x = AndroidPxToDip(motion.GetX());
                double y = AndroidPxToDip(motion.GetY());
                _androidLastTouchX = x;
                _androidLastTouchY = y;
                _androidSingleTouchReady = true;

                if (!IsZoomed)
                    BeginDismiss(y);
                break;
            }

            case Android.Views.MotionEventActions.PointerDown:
                // Two fingers now own the gesture. Stop the 1-finger drag so
                // pinch and pan/dismiss never write transforms at the same time.
                _dismissGestureActive = false;
                _androidSingleTouchReady = false;
                if (!IsZoomed)
                {
                    ApplyTransform(MinScale, 0, 0);
                    boxFullImage.Opacity = OverlayOpacity;
                }
                break;

            case Android.Views.MotionEventActions.Move:
                if (motion.PointerCount == 1 && _androidScaleDetector?.IsInProgress != true)
                {
                    double x = AndroidPxToDip(motion.GetX());
                    double y = AndroidPxToDip(motion.GetY());

                    // After a pinch ends one finger may remain on screen. Seed
                    // the position first so the image never jumps on that frame.
                    if (!_androidSingleTouchReady)
                    {
                        _androidLastTouchX = x;
                        _androidLastTouchY = y;
                        _androidSingleTouchReady = true;
                        break;
                    }

                    if (IsZoomed)
                    {
                        PanZoomed(x - _androidLastTouchX, y - _androidLastTouchY);
                    }
                    else
                    {
                        UpdateDismiss(y);
                    }

                    _androidLastTouchX = x;
                    _androidLastTouchY = y;
                }
                break;

            case Android.Views.MotionEventActions.PointerUp:
                // Do not reuse coordinates from the two-finger gesture for the
                // next one-finger drag. The next MOVE will seed them cleanly.
                _androidSingleTouchReady = false;
                break;

            case Android.Views.MotionEventActions.Up:
            {
                _androidGestureView?.Parent?.RequestDisallowInterceptTouchEvent(false);
                double y = AndroidPxToDip(motion.GetY());

                if (!IsZoomed && _androidScaleDetector?.IsInProgress != true)
                    _ = EndDismissAsync(y);

                _androidSingleTouchReady = false;
                break;
            }

            case Android.Views.MotionEventActions.Cancel:
                _androidGestureView?.Parent?.RequestDisallowInterceptTouchEvent(false);
                _dismissGestureActive = false;
                _androidSingleTouchReady = false;
                if (!IsZoomed)
                    _ = RestoreNormalPositionAsync();
                break;
        }

        e.Handled = true;
    }

    private sealed class AndroidScaleListener : Android.Views.ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        private readonly ImagePreviewView _owner;

        public AndroidScaleListener(ImagePreviewView owner)
        {
            _owner = owner;
        }

        public override bool OnScaleBegin(Android.Views.ScaleGestureDetector detector)
        {
            _owner.BeginPinch(
                _owner.AndroidPxToDip(detector.FocusX),
                _owner.AndroidPxToDip(detector.FocusY));
            return true;
        }

        public override bool OnScale(Android.Views.ScaleGestureDetector detector)
        {
            _owner.UpdatePinch(detector.ScaleFactor, _owner.AndroidPxToDip(detector.FocusX), _owner.AndroidPxToDip(detector.FocusY));
            return true;
        }

        public override void OnScaleEnd(Android.Views.ScaleGestureDetector detector)
        {
            _owner.EndPinch();
        }
    }

    private sealed class AndroidGestureListener : Android.Views.GestureDetector.SimpleOnGestureListener
    {
        private readonly ImagePreviewView _owner;

        public AndroidGestureListener(ImagePreviewView owner)
        {
            _owner = owner;
        }

        public override bool OnDown(Android.Views.MotionEvent e)
        {
            return true;
        }

        public override bool OnDoubleTap(Android.Views.MotionEvent e)
        {
            _ = _owner.ToggleDoubleTapZoomAsync(_owner.AndroidPxToDip(e.GetX()), _owner.AndroidPxToDip(e.GetY()));
            return true;
        }

    }
#endif

#if IOS
    private void SetupIosGestures()
    {
        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnIosPinchUpdated;
        gestureSurface.GestureRecognizers.Add(pinch);

        var pan = new PanGestureRecognizer { TouchPoints = 1 };
        pan.PanUpdated += OnIosPanUpdated;
        gestureSurface.GestureRecognizers.Add(pan);

        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += OnIosDoubleTapped;
        gestureSurface.GestureRecognizers.Add(doubleTap);
    }

    private void OnIosPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (_isAnimating)
            return;

        switch (e.Status)
        {
            case GestureStatus.Started:
            {
                double focusX = e.ScaleOrigin.X * GetViewportWidth();
                double focusY = e.ScaleOrigin.Y * GetViewportHeight();
                BeginPinch(focusX, focusY);
                _iosLastPinchScale = 1.0;
                break;
            }

            case GestureStatus.Running:
                if (!_isPinching)
                    return;

                double incrementalScale = _iosLastPinchScale <= 0 ? 1.0 : e.Scale / _iosLastPinchScale;
                _iosLastPinchScale = e.Scale;

                double focusX = e.ScaleOrigin.X * GetViewportWidth();
                double focusY = e.ScaleOrigin.Y * GetViewportHeight();
                UpdatePinch(incrementalScale, focusX, focusY);
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                EndPinch();
                _iosLastPinchScale = 1.0;
                break;
        }
    }

    private void OnIosPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating || _isPinching)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _iosPanStartedZoomed = IsZoomed;
                _iosLastPanX = 0;
                _iosLastPanY = 0;

                if (!_iosPanStartedZoomed)
                    BeginDismiss(0);
                break;

            case GestureStatus.Running:
                if (_iosPanStartedZoomed && IsZoomed)
                {
                    double deltaX = e.TotalX - _iosLastPanX;
                    double deltaY = e.TotalY - _iosLastPanY;
                    _iosLastPanX = e.TotalX;
                    _iosLastPanY = e.TotalY;
                    PanZoomed(deltaX, deltaY);
                }
                else if (!IsZoomed)
                {
                    UpdateDismiss(e.TotalY);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (!_iosPanStartedZoomed && !IsZoomed)
                    _ = EndDismissAsync(e.TotalY);

                _iosLastPanX = 0;
                _iosLastPanY = 0;
                _iosPanStartedZoomed = false;
                break;
        }
    }

    private void OnIosDoubleTapped(object? sender, TappedEventArgs e)
    {
        var point = e.GetPosition(gestureSurface);
        double focusX = point?.X ?? (GetViewportWidth() / 2.0);
        double focusY = point?.Y ?? (GetViewportHeight() / 2.0);
        _ = ToggleDoubleTapZoomAsync(focusX, focusY);
    }
#endif
}