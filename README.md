# 🖼️ ImageViewer — WPF Image Processing App

A desktop image processing application built with **C#**, **WPF**, and **OpenCvSharp** as part of a learning journey into Computer Vision. Every feature is implemented from scratch to understand the underlying OpenCV concepts — not just use them.

> **Portfolio Project 1 of N** — Image Basics & Core Operations

---

## 📸 Screenshot

<!-- Add a screenshot of your app here after taking one -->
<!-- Drag your screenshot into the GitHub repo and replace the line below -->
![ImageViewer App Screenshot](docs/screenshot.png)

---

## ✨ Features

| Category | Operations |
|---|---|
| **Color Space** | Grayscale, HSV, BGR → RGB conversion |
| **Flip** | Horizontal, Vertical, Both axes |
| **Filters** | Gaussian Blur (adjustable kernel), Canny Edge Detection, Binary Threshold, Sharpen |
| **Transform** | Resize (slider), Crop Centre 50% |
| **Drawing** | Rectangle, Circle, Text overlay |
| **Analysis** | Histogram Equalization, Centre Pixel Inspector |
| **File** | Open (JPG, PNG, BMP, TIFF), Save, Reset to Original |

### 🔄 Toggle Behaviour
Every operation works as a **toggle** — click once to apply, click again to undo. No need to reset the whole image just to reverse one step.

---

## 🧠 Concepts Learned

This project was built to understand the following Computer Vision fundamentals:

- **What an image is** — a 2D matrix of pixel values (0–255 per channel)
- **BGR vs RGB** — why OpenCV uses BGR order and how to convert
- **Color spaces** — BGR, Grayscale, HSV and when to use each
- **Kernels & convolution** — how blur, sharpen, and edge detection work mathematically
- **Gaussian blur** — kernel size, why it must be odd, sigma vs size
- **Canny edge detection** — the 4-step pipeline (blur → gradient → non-max suppression → threshold)
- **Histogram equalization** — redistributing pixel intensity for contrast enhancement
- **Mat class** — OpenCV's core data structure, Clone vs assignment, channels, types

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| C# | .NET 8 | Application language |
| WPF | .NET 8 Windows | Desktop UI framework |
| OpenCvSharp4 | 4.13.0 | OpenCV bindings for C# |
| OpenCvSharp4.WpfExtensions | 4.13.0 | Mat → BitmapSource conversion |
| OpenCvSharp4.runtime.win | 4.13.0 | Native OpenCV Windows runtime |

---

## 🚀 Getting Started

### Prerequisites

- Windows 10/11
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with **.NET desktop development** workload
- .NET 8 SDK

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/YOUR_USERNAME/ImageViewer.git

# 2. Open in Visual Studio
# Double-click ImageViewer.slnx

# 3. Restore NuGet packages
# Visual Studio does this automatically on first build

# 4. Build and run
# Press F5 or Ctrl+F5
```

### NuGet Packages (auto-restored)

```
OpenCvSharp4                 4.13.0.20260330
OpenCvSharp4.runtime.win     4.13.0.20260302
OpenCvSharp4.WpfExtensions   4.13.0.20260330
```

---

## 📁 Project Structure

```
ImageViewer/
├── ImageViewer.slnx          # Solution file
└── ImageViewer/
    ├── MainWindow.xaml        # UI layout — dark theme, 3-panel design
    ├── MainWindow.xaml.cs     # Button handlers, toggle logic, display
    ├── ImageProcessor.cs      # All OpenCV operations (pure logic, no UI)
    ├── App.xaml               # Application entry point
    └── ImageViewer.csproj     # Project config + NuGet references
```

### Architecture Decision

`ImageProcessor.cs` is kept **completely separate** from the UI. Every method takes a `Mat` and returns a new `Mat` — no UI references, no side effects. This makes the processing logic reusable, testable, and easy to understand.

---

## 🔍 Key Code — How Operations Work

### Toggle (apply / undo with same button)

```csharp
private void Apply(Func<Mat> operation, string opName, string? newColorSpace = null)
{
    // Same button clicked again → restore previous state
    if (_lastOp == opName && _previous != null)
    {
        _current         = _previous.Clone();
        _colorSpaceLabel = _lastColorSpaceLabel;
        _previous        = null;
        _lastOp          = "";
        DisplayMat(_current);
        return;
    }

    // Save current before applying
    _previous            = _current.Clone();
    _lastColorSpaceLabel = _colorSpaceLabel;
    _lastOp              = opName;

    _current = operation();
    DisplayMat(_current);
}
```

### Sharpen Kernel (3×3 convolution)

```csharp
// 0  -1   0
//-1   5  -1    ← centre gets 5×, each direct neighbour subtracted
// 0  -1   0
//
// Flat areas: 5×v - 4×v = v  (unchanged)
// Edges: centre amplified, difference boosted → sharpening
var kernel = new Mat(3, 3, MatType.CV_32F);
kernel.Set<float>(1, 1, 5f);
// ... fill neighbours with -1
Cv2.Filter2D(src, dst, -1, kernel);
```

### Channel Guard (prevent invalid conversions)

```csharp
// Every operation that needs BGR checks first
private static Mat EnsureBGR(Mat src)
{
    if (src.Channels() != 1) return src;
    var bgr = new Mat();
    Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
    return bgr;
}
```

---

## 🗺️ Learning Roadmap

This is **Project 1** in a series of Computer Vision portfolio projects:

- [x] **Project 1** — Image basics, color spaces, filters, transforms (this repo)
- [ ] **Project 2** — Live color filter with HSV masking and trackbars
- [ ] **Project 3** — Edge detector with Sobel + Canny comparison
- [ ] **Project 4** — Shape & contour detector
- [ ] **Project 5** — Real-time face detector (Haar cascades / DNN)
- [ ] **Project 6** — Document scanner (perspective transform)
- [ ] **Project 7** — Object tracker (CSRT/KCF)

---

## 📚 What I Learned Building This

### The hardest bug
Clicking a color-space button twice caused an OpenCV exception:
`Invalid number of channels: VScn::contains(scn) where scn is 1`

**Root cause:** Calling `BGR2GRAY` on an image already in grayscale (1 channel).
**Fix:** `EnsureBGR()` guard on every operation that requires a colour input, plus early return in `ToGrayscale()` when already 1 channel.

### The most surprising thing
OpenCV uses **BGR** not RGB. A pixel that looks red on screen is stored as `[0, 0, 255]` — Blue=0, Green=0, Red=255. Getting this wrong makes every colour operation produce wrong results.

### The most useful concept
The `Mat` class assignment does **not** copy data:
```csharp
Mat b = a;        // b and a share memory — dangerous
Mat b = a.Clone(); // independent copy — safe
```
Always `.Clone()` before modifying.

---

## 🤝 Contributing

This is a personal learning project but PRs and suggestions are welcome. If you spot a better way to implement any operation, open an issue.

---

## 📄 License

MIT License — free to use, learn from, and build on.

---

## 👤 Author

Built by **[Your Name]** as part of a self-directed Computer Vision learning path.

- GitHub: [@YOUR_USERNAME](https://github.com/YOUR_USERNAME)
- Learning path: C# → WPF → OpenCvSharp → C++ OpenCV → Python CV
