using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.IO;
using System.Windows;


namespace TLHFaceTrack
{
    public class CamModel : IDisposable
    {
        private Task _camTask;
        private System.Drawing.Bitmap _lastFrame;
        private CancellationTokenSource _cancelToken;
        private readonly Image _passedInImage;

        #region construction/destruction
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="imageFromMainWindow">the image top be updated from the main window</param>
        public CamModel( Image imageFromMainWindow)
        {
            _passedInImage = imageFromMainWindow;
        }


        /// <summary>
        /// stops any in progress webcam capture
        /// </summary>
        public void Dispose()
        {
            // shutting down the webcam task, if its running
            _cancelToken?.Cancel();
        }
        #endregion

        #region control methods for model
        /// <summary>
        /// called to start webcam capture
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            // this should never happen, nut jic
            if (_camTask != null && !_camTask.IsCompleted)
                return;

            if (_cancelToken != null)
            {
                _cancelToken.Dispose();
                _cancelToken = null;
            }
            _cancelToken = new CancellationTokenSource();

            _camTask = Task.Run(async () =>
            {
                try
                {
                    // opening webcam interface, for demo assuming 0 device
                   var camStream = new VideoCapture(0);

                    using (var camFrame = new Mat())
                    {
                        // keep going till someone tells you to stop
                        while (!_cancelToken.IsCancellationRequested)
                        {
                            camStream.Read(camFrame);

                            if (!camFrame.Empty())
                            {
                                // converting the latest frame to a bitmap
                                _lastFrame = BitmapConverter.ToBitmap(camFrame);
                                var bitmapData = _lastFrame.LockBits(new System.Drawing.Rectangle(0, 0, _lastFrame.Width, _lastFrame.Height),
                                    System.Drawing.Imaging.ImageLockMode.ReadOnly, _lastFrame.PixelFormat);
                                var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(bitmapData.Width, bitmapData.Height,
                                    _lastFrame.HorizontalResolution, _lastFrame.VerticalResolution,
                                    System.Windows.Media.PixelFormats.Bgr24, null,
                                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
                                _lastFrame.UnlockBits(bitmapData);
                                var lastFrameBitmapImage = bitmapSource;

                                // updating the displayed image
                                lastFrameBitmapImage.Freeze();
                                _passedInImage.Dispatcher.Invoke(() => _passedInImage.Source = lastFrameBitmapImage);
                            }

                            // slowing loop to approx 30 FPS
                            await Task.Delay(33);
                        }
                    }

                    camStream?.Dispose();
                }
                finally
                {
                }

            }, _cancelToken.Token);

            if (_camTask.IsFaulted)
            {
                // To let the exceptions exit
                await _camTask;
            }
        }

        /// <summary>
        /// stops active webcam capture
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            // in the process of stopping
            if (_cancelToken.IsCancellationRequested)
                return;

            // stopping webcam capture
            if (!_camTask.IsCompleted)
            {
                _cancelToken.Cancel();

                // Wait for it
                await _camTask;
            }
        }
        #endregion
    }

}
