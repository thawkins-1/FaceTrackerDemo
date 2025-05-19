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
        private bool _runFaceRecognition = true;
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
                    // used to detect a face and put a blue rect around it
                    var faceDetect = new CascadeClassifier(@"../../Detection/haarcascade_frontalface_alt.xml");
                    var eyeDetect = new CascadeClassifier(@"../../Detection/haarcascade_eye.xml");
                    var faceHalo = Scalar.FromRgb(0, 0, 255);

                    // opening webcam interface, for demo assuming 0 device
                    using (var camStream = new VideoCapture(0))
                    using(var camFrame = new Mat())
                    using(var grayImage = new Mat())
                    using(var foundGreyImage  = new Mat())
                    {
                        // keep going till someone tells you to stop
                        while (!_cancelToken.IsCancellationRequested)
                        {
                            camStream.Read(camFrame);

                            if (!camFrame.Empty())
                            {
                                if (_runFaceRecognition)
                                {
                                    // converting frame to grayscale and performing face recognition
                                    Cv2.CvtColor(camFrame, grayImage, ColorConversionCodes.BGRA2GRAY);
                                    Cv2.EqualizeHist(grayImage, grayImage);
                                    var faces = faceDetect.DetectMultiScale(
                                        image: grayImage,
                                        minSize: new OpenCvSharp.Size(30, 30));

                                    // processing detected faces, if any found
                                    foreach (var face in faces)
                                    {
                                        using (var foundFaceImage = new Mat(camFrame, face))
                                        {
                                            // adding rectangle around face
                                            Cv2.Rectangle(camFrame, face, faceHalo, 3);

                                            // performing eye recogntion
                                            Cv2.CvtColor(foundFaceImage, foundGreyImage, ColorConversionCodes.BGRA2GRAY);
                                            var eyes = eyeDetect.DetectMultiScale(
                                                image: foundGreyImage,
                                                minSize: new OpenCvSharp.Size(30, 30));

                                            // processing eyes, if any
                                            foreach (var eye in eyes)
                                            {
                                                // circles around eyes
                                                var eyeCenter = new OpenCvSharp.Point
                                                {
                                                    X = (int)(Math.Round(eye.X + eye.Width * 0.5, MidpointRounding.ToEven) + face.Left),
                                                    Y = (int)(Math.Round(eye.Y + eye.Height * 0.5, MidpointRounding.ToEven) + face.Top)
                                                };
                                                var eyeRadius = Math.Round((eye.Width + eye.Height) * 0.25, MidpointRounding.ToEven);
                                                Cv2.Circle(camFrame, eyeCenter, (int)eyeRadius, faceHalo, thickness: 2);
                                            }
                                        }
                                    }
                                } // end of if (_runFaceRecognition)
                                
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

        /// <summary>
        /// sets bool to turn on and off face recogntion
        /// </summary>
        /// <param name="shouldRun"></param>
        public void RunFaceRecognition(bool shouldRun)
        {
            _runFaceRecognition = shouldRun;
        }
        #endregion
    }
}
