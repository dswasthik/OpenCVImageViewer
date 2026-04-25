using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;   // requires OpenCvSharp4.WpfExtensions NuGet
using System;
using System.Windows;

// Resolve all ambiguities with System.Windows types up front
using OcvPoint = OpenCvSharp.Point;
using OcvRect = OpenCvSharp.Rect;
using OcvSize = OpenCvSharp.Size;

namespace ImageProcessingApp
{
    public partial class MainWindow : System.Windows.Window
    {
        private Mat? _original;
        private Mat? _current;
        private string _colorSpaceLabel = "BGR";

        public MainWindow()
        {
            InitializeComponent();
            SliderBlur.ValueChanged += (s, e) =>
                TxtBlurVal.Text = $"Kernel: {(int)SliderBlur.Value}";
        }

        // ── File ──────────────────────────────────────────────

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|All files|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            _original = Cv2.ImRead(dlg.FileName, ImreadModes.Color);
            if (_original.Empty())
            {
                MessageBox.Show("Could not load image.", "Error");
                return;
            }

            _current = _original.Clone();
            _colorSpaceLabel = "BGR";
            DisplayMat(_current);
            UpdateInfo();
            SetControlsEnabled(true);
            SetStatus($"Opened: {System.IO.Path.GetFileName(dlg.FileName)}", "Image loaded");
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            var dlg = new SaveFileDialog
            {
                Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp",
                FileName = "processed_image"
            };
            if (dlg.ShowDialog() != true) return;

            Cv2.ImWrite(dlg.FileName, _current);
            SetStatus($"Saved: {System.IO.Path.GetFileName(dlg.FileName)}", "✓ Saved");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_original == null) return;
            _current = _original.Clone();
            _colorSpaceLabel = "BGR";
            DisplayMat(_current);
            UpdateInfo();
            SetStatus("Reset to original", "Original restored");
        }

        // ── Color Space ───────────────────────────────────────

        private void Grayscale_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.ToGrayscale(_current!), "Grayscale", "GRAY");

        private void HSV_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.ToHSV(_current!), "HSV", "HSV");

        private void BGR2RGB_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.BGRtoRGB(_current!), "BGR→RGB", "RGB");

        // ── Flip ──────────────────────────────────────────────

        private void FlipH_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.Flip(_current!, FlipMode.Y), "Flip Horizontal");

        private void FlipV_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.Flip(_current!, FlipMode.X), "Flip Vertical");

        private void FlipBoth_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.Flip(_current!, FlipMode.XY), "Flip Both");

        // ── Filters ───────────────────────────────────────────

        private void GaussianBlur_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.GaussianBlur(_current!, (int)SliderBlur.Value),
                  $"Gaussian Blur (k={(int)SliderBlur.Value})");

        private void Canny_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.CannyEdges(_current!), "Canny Edges", "GRAY");

        private void Threshold_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.Threshold(_current!), "Threshold", "GRAY");

        private void Sharpen_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.Sharpen(_current!), "Sharpen");

        // ── Transform ─────────────────────────────────────────

        private void ShowResize_Click(object sender, RoutedEventArgs e)
        {
            ResizeBar.Visibility = Visibility.Visible;
            BtnApplyResize.IsEnabled = true;
            SetStatus("Drag the slider then click Apply Resize", "Resize mode");
        }

        private void SliderScale_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtScale != null)
                TxtScale.Text = $"{(int)SliderScale.Value}%";
        }

        private void ApplyResize_Click(object sender, RoutedEventArgs e)
        {
            Apply(() => ImageProcessor.Resize(_current!, SliderScale.Value),
                  $"Resize {(int)SliderScale.Value}%");
            ResizeBar.Visibility = Visibility.Collapsed;
        }

        private void CropCenter_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.CropCenter(_current!), "Crop Center 50%");

        // ── Drawing ───────────────────────────────────────────

        private void DrawRect_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.DrawRectangle(_current!), "Draw Rectangle");

        private void DrawCircle_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.DrawCircle(_current!), "Draw Circle");

        private void DrawText_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.DrawText(_current!), "Draw Text");

        // ── Analysis ──────────────────────────────────────────

        private void EqualizeHist_Click(object sender, RoutedEventArgs e) =>
            Apply(() => ImageProcessor.EqualizeHistogram(_current!),
                  "Equalize Histogram", "GRAY");

        private void PixelInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            string info = ImageProcessor.GetPixelInfo(_current);
            MessageBox.Show(info, "Pixel Info", MessageBoxButton.OK);
            SetStatus(info, "Pixel read");
        }

        // ── Core Helpers ──────────────────────────────────────

        private void Apply(Func<Mat> operation, string opName,
                           string? newColorSpace = null)
        {
            if (_current == null) return;
            try
            {
                _current = operation();
                if (newColorSpace != null) _colorSpaceLabel = newColorSpace;
                DisplayMat(_current);
                UpdateInfo();
                SetStatus($"Applied: {opName}", $"✓ {opName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed:\n{ex.Message}", "Error");
            }
        }

        private void DisplayMat(Mat mat)
        {
            // BitmapSourceConverter works without WpfExtensions as fallback too
            ImgDisplay.Source = BitmapSourceConverter.ToBitmapSource(mat);
            EmptyState.Visibility = Visibility.Collapsed;
            ImageBorder.Visibility = Visibility.Visible;
        }

        private void UpdateInfo()
        {
            if (_current == null) return;
            TxtWidth.Text = $"Width   : {_current.Width}px";
            TxtHeight.Text = $"Height  : {_current.Height}px";
            TxtChannels.Text = $"Channels: {_current.Channels()}";
            TxtColorSpace.Text = $"Space   : {_colorSpaceLabel}";
        }

        private void SetStatus(string left, string right)
        {
            TxtStatus.Text = left;
            TxtOp.Text = right;
        }

        private void SetControlsEnabled(bool on)
        {
            BtnSave.IsEnabled = on;
            BtnReset.IsEnabled = on;
            BtnGray.IsEnabled = on;
            BtnHSV.IsEnabled = on;
            BtnRGB.IsEnabled = on;
            BtnFlipH.IsEnabled = on;
            BtnFlipV.IsEnabled = on;
            BtnFlipB.IsEnabled = on;
            BtnBlur.IsEnabled = on;
            BtnCanny.IsEnabled = on;
            BtnThreshold.IsEnabled = on;
            BtnSharpen.IsEnabled = on;
            BtnDrawRect.IsEnabled = on;
            BtnDrawCircle.IsEnabled = on;
            BtnDrawText.IsEnabled = on;
            BtnResize.IsEnabled = on;
            BtnCrop.IsEnabled = on;
            BtnHist.IsEnabled = on;
            BtnPixel.IsEnabled = on;
            ResizeBar.Visibility = Visibility.Collapsed;
        }
    }
}