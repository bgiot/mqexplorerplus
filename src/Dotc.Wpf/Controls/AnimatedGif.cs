using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Dotc.Wpf.Controls
{
    public class AnimatedGif : System.Windows.Controls.Image
    {
        private bool _isInitialized;
        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;

        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        private static Uri GetUri(ImageSource image)
        {
            var bmp = image as BitmapImage;
            if (bmp != null && bmp.UriSource != null)
            {
                if (bmp.UriSource.IsAbsoluteUri)
                    return bmp.UriSource;
                if (bmp.BaseUri != null)
                    return new Uri(bmp.BaseUri, bmp.UriSource);
            }
            var frame = image as BitmapFrame;
            if (frame != null)
            {
                string s = frame.ToString();
                if (s != frame.GetType().FullName)
                {
                    Uri fUri;
                    if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out fUri))
                    {
                        if (fUri.IsAbsoluteUri)
                            return fUri;
                        if (frame.BaseUri != null)
                            return new Uri(frame.BaseUri, fUri);
                    }
                }
            }
            return null;
        }
        private void Initialize()
        {
            if (this.GifSource == null) return;

            _gifDecoder = new GifBitmapDecoder(GetUri(this.GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, _gifDecoder.Frames.Count / 10, (int)((_gifDecoder.Frames.Count / 10.0 - _gifDecoder.Frames.Count / 10) * 1000))));
            _animation.RepeatBehavior = RepeatBehavior.Forever;
            this.Source = _gifDecoder.Frames[0];

            _isInitialized = true;

            if (AutoStart && Visibility == Visibility.Visible) StartAnimation();
        }

        static AnimatedGif()
        {
            VisibilityProperty.OverrideMetadata(typeof(AnimatedGif),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                ((AnimatedGif)sender).StartAnimation();
            }
            else
            {
                ((AnimatedGif)sender).StopAnimation();
            }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(AnimatedGif), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as AnimatedGif;
            gifImage.Source = gifImage._gifDecoder.Frames[(int)ev.NewValue];
        }

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(AnimatedGif), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as AnimatedGif).StartAnimation();
        }

        public ImageSource GifSource
        {
            get { return (ImageSource)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(ImageSource), typeof(AnimatedGif), new UIPropertyMetadata(null, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as AnimatedGif).Initialize();
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }
    }
}
