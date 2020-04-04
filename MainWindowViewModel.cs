using OpenCvSharp;
using OpenCvSharp.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace CameraOpenCV
{
    public class MainWindowViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<WriteableBitmap> CameraCaptureSource { get; set; }
        public ReactiveCommand ShutterCommand { get; set; }

        private readonly Camera Camera;
        private readonly CompositeDisposable Disposables = new CompositeDisposable();
        public MainWindowViewModel()
        {
            this.Camera = new Camera().AddTo(Disposables);
            this.CameraCaptureSource = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .ObserveOnUIDispatcher()
                .Select(x => Camera.Capture().ToWriteableBitmap())
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);

            ShutterCommand = new ReactiveCommand();
            ShutterCommand.Subscribe(x =>
            {
                var bitmap = CameraCaptureSource.Value;
                var result = new SaveFileDialog();
                if (result.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(result.FileName, bitmap.ToMat().ToBytes()); 
                }
            });
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }
    }

    public class Camera : IDisposable
    {
        private readonly VideoCapture capture;
        private readonly Mat frame;

        public Camera()
        {
            capture = new VideoCapture(0);
            frame = new Mat();
        }

        public Mat Capture()
        {
            capture.Read(frame);
            if (frame.Empty())
                return null;

            return frame.Clone();
        }

        public void Dispose()
        {
            capture?.Dispose();
            frame?.Dispose();
        }
    }
}
