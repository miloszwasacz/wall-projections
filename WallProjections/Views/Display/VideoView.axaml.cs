using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using WallProjections.Models.Interfaces;
using WallProjections.ViewModels.Interfaces.Display;

namespace WallProjections.Views.Display
{
    public partial class VideoView : UserControl
    {
        public VideoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the video player handle in the viewmodel.
        /// </summary>
        /// <param name="sender">The sender of the event (unused).</param>
        /// <param name="e">The event arguments (unused).</param>
        internal async void OnPlayerLoaded(object? sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            var viewModel = DataContext as IVideoViewModel;
            var handle = VideoViewer.Handle;
            if (viewModel?.MediaPlayer is null || handle is null) return;

            viewModel.MediaPlayer.SetHandle(handle);
            viewModel.MarkLoaded();
        }
    }
}

namespace WallProjections.Views.Converters
{
    /// <summary>
    /// A converter that calculates the aspect ratio of a NativeVideoView
    /// based on the size of the video and the available space.
    /// </summary>
    public abstract class AspectRatioConverter : IMultiValueConverter
    {
        /// <summary>
        /// The default aspect ratio of a video.
        /// </summary>
        public static readonly (uint Width, uint Height) DefaultAspectRatio = (16, 9);

        /// <summary>
        /// Converts (Width, Height, VideoSize) to the required MaxWidth or MaxHeight of a NativeVideoView,
        /// where Width and Height are the available space (e.g. the size of a parent control) and
        /// VideoSize is the size of the video from the associated <see cref="IMediaPlayer" />
        /// </summary>
        /// <inheritdoc />
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 3) return double.PositiveInfinity;
            if (values[0] is not double width || values[1] is not double height) return double.PositiveInfinity;

            var video = values[2] as (uint Width, uint Height)? ?? DefaultAspectRatio;
            return Calculate(video, width, height);
        }

        /// <summary>
        /// Calculates the required MaxWidth or MaxHeight of a NativeVideoView
        /// given the size of the video and the available space.
        /// </summary>
        /// <param name="videoSize">The size (Width, Height) of the video</param>
        /// <param name="availableWidth">The max available width for the video</param>
        /// <param name="availableHeight">The max available height for the video</param>
        /// <returns>The required MaxWidth or MaxHeight for the video</returns>
        protected abstract double Calculate(
            (uint Width, uint Height) videoSize,
            double availableWidth,
            double availableHeight
        );
    }

    /// <summary>
    /// A converter that calculates the MaxHeight of a NativeVideoView
    /// based on the size of the video and the available space.
    /// </summary>
    public class HeightConverter : AspectRatioConverter
    {
        /// <summary>
        /// Calculates the required MaxHeight of a NativeVideoView
        /// given the size of the video and the available space.
        /// </summary>
        /// <param name="videoSize">The size (Width, Height) of the video</param>
        /// <param name="availableWidth">The max available width for the video</param>
        /// <param name="availableHeight">The max available height for the video</param>
        /// <returns>The required MaxHeight for the video</returns>
        protected override double Calculate(
            (uint Width, uint Height) videoSize,
            double availableWidth,
            double availableHeight
        ) => availableWidth * videoSize.Height / videoSize.Width;
    }

    /// <summary>
    /// A converter that calculates the MaxWidth of a NativeVideoView
    /// based on the size of the video and the available space.
    /// </summary>
    public class WidthConverter : AspectRatioConverter
    {
        /// <summary>
        /// Calculates the required MaxWidth of a NativeVideoView
        /// given the size of the video and the available space.
        /// </summary>
        /// <param name="videoSize">The size (Width, Height) of the video</param>
        /// <param name="availableWidth">The max available width for the video</param>
        /// <param name="availableHeight">The max available height for the video</param>
        /// <returns>The required MaxWidth for the video</returns>
        protected override double Calculate(
            (uint Width, uint Height) videoSize,
            double availableWidth,
            double availableHeight
        ) => availableHeight * videoSize.Width / videoSize.Height;
    }
}
