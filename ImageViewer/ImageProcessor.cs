using OpenCvSharp;

// Fully qualify to avoid ALL ambiguity with System.Windows types
using OcvPoint = OpenCvSharp.Point;
using OcvRect = OpenCvSharp.Rect;
using OcvSize = OpenCvSharp.Size;

namespace ImageProcessingApp
{
    public static class ImageProcessor
    {
        // ── Color Space ───────────────────────────────────────

        public static Mat ToGrayscale(Mat src)
        {
            // Guard: already grayscale, just return a clone — nothing to do
            if (src.Channels() == 1)
                return src.Clone();

            var dst = new Mat();
            Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
            return dst;
        }

        public static Mat ToHSV(Mat src)
        {
            // Guard: if already grayscale, convert to BGR first
            var input = EnsureBGR(src);
            var dst = new Mat();
            Cv2.CvtColor(input, dst, ColorConversionCodes.BGR2HSV);
            return dst;
        }


        public static Mat BGRtoRGB(Mat src)
        {
            // Guard: if already grayscale, convert to BGR first
            var input = EnsureBGR(src);
            var dst = new Mat();
            Cv2.CvtColor(input, dst, ColorConversionCodes.BGR2RGB);
            return dst;
        }

        // ── Flip ──────────────────────────────────────────────

        public static Mat Flip(Mat src, FlipMode mode)
        {
            var dst = new Mat();
            Cv2.Flip(src, dst, mode);
            return dst;
        }

        // ── Filters ───────────────────────────────────────────

        public static Mat GaussianBlur(Mat src, int kernelSize)
        {
            if (kernelSize % 2 == 0) kernelSize++;
            var dst = new Mat();
            Cv2.GaussianBlur(src, dst, new OcvSize(kernelSize, kernelSize), 0);
            return dst;
        }

        public static Mat CannyEdges(Mat src)
        {
            var gray = src.Channels() == 1 ? src : ToGrayscale(src);
            var dst = new Mat();
            Cv2.Canny(gray, dst, 50, 150);
            return dst;
        }

        public static Mat Threshold(Mat src)
        {
            var gray = src.Channels() == 1 ? src : ToGrayscale(src);
            var dst = new Mat();
            Cv2.Threshold(gray, dst, 127, 255, ThresholdTypes.Binary);
            return dst;
        }

        public static Mat Sharpen(Mat src)
        {
            // Guard: sharpen needs BGR, not grayscale
            var input = EnsureBGR(src);
            var kernel = new Mat(3, 3, MatType.CV_32F);
            kernel.Set<float>(0, 0, 0f);
            kernel.Set<float>(0, 1, -1f);
            kernel.Set<float>(0, 2, 0f);
            kernel.Set<float>(1, 0, -1f);
            kernel.Set<float>(1, 1, 5f);
            kernel.Set<float>(1, 2, -1f);
            kernel.Set<float>(2, 0, 0f);
            kernel.Set<float>(2, 1, -1f);
            kernel.Set<float>(2, 2, 0f);

            var dst = new Mat();
            Cv2.Filter2D(input, dst, -1, kernel);
            return dst;
        }

        // ── Transform ─────────────────────────────────────────

        public static Mat Resize(Mat src, double scalePct)
        {
            double scale = scalePct / 100.0;
            var dst = new Mat();
            Cv2.Resize(src, dst,
                new OcvSize((int)(src.Width * scale), (int)(src.Height * scale)));
            return dst;
        }

        public static Mat CropCenter(Mat src, double fraction = 0.5)
        {
            int newW = (int)(src.Width * fraction);
            int newH = (int)(src.Height * fraction);
            int x = (src.Width - newW) / 2;
            int y = (src.Height - newH) / 2;

            var roi = new OcvRect(x, y, newW, newH);
            return new Mat(src, roi);
        }

        // ── Drawing ───────────────────────────────────────────

        public static Mat DrawRectangle(Mat src)
        {
            // Guard: drawing needs a colour image
            var dst = EnsureBGR(src).Clone();
            Cv2.Rectangle(dst,
                new OcvPoint(30, 30),
                new OcvPoint(dst.Width - 30, dst.Height - 30),
                new Scalar(0, 255, 0), 3);
            return dst;
        }

        public static Mat DrawCircle(Mat src)
        {
            var dst = EnsureBGR(src).Clone();
            var center = new OcvPoint(dst.Width / 2, dst.Height / 2);
            int radius = System.Math.Min(dst.Width, dst.Height) / 4;
            Cv2.Circle(dst, center, radius, new Scalar(255, 80, 0), 3);
            return dst;
        }


        public static Mat DrawText(Mat src, string text = "OpenCvSharp WPF")
        {
            var dst = EnsureBGR(src).Clone();
            Cv2.PutText(dst, text,
                new OcvPoint(20, 50),
                HersheyFonts.HersheySimplex,
                1.2,
                new Scalar(0, 220, 255), 2);
            return dst;
        }

        // ── Analysis ──────────────────────────────────────────

        public static Mat EqualizeHistogram(Mat src)
        {
            var gray = src.Channels() == 1 ? src : ToGrayscale(src);
            var dst = new Mat();
            Cv2.EqualizeHist(gray, dst);
            return dst;
        }

        public static string GetPixelInfo(Mat src)
        {
            int cx = src.Width / 2;
            int cy = src.Height / 2;

            if (src.Channels() == 1)
            {
                byte val = src.At<byte>(cy, cx);
                return $"Center pixel — Gray: {val}";
            }

            Vec3b px = src.At<Vec3b>(cy, cx);
            return $"Center pixel — B:{px.Item0}  G:{px.Item1}  R:{px.Item2}";
        }

        // ── Private helpers ───────────────────────────────────

        private static Mat EnsureBGR(Mat src)
        {
            if (src.Channels() != 1) return src;
            var bgr = new Mat();
            Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
            return bgr;
        }
    }
}