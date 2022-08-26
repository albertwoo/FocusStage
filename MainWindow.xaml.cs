using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace FocusStage;

public partial class MainWindow : Window
{
    private readonly double dpiX;
    private readonly double dpiY;
    private bool renderBlank;


    public MainWindow()
    {
        InitializeComponent();

        SourceInitialized += MainWindow_SourceInitialized;
        Closed += MainWindow_Closed;
        SizeChanged += MainWindow_SizeChanged;


        windowChrome.ResizeBorderThickness = new Thickness(3, 3, 3, 3);
        windowChrome.CaptionHeight = 0;

        using var source = new HwndSource(new HwndSourceParameters());
        dpiX = source.CompositionTarget.TransformToDevice.M11;
        dpiY = source.CompositionTarget.TransformToDevice.M22;

        LoopUpdateCanvas();
    }


    private async void LoopUpdateCanvas()
    {
        while (true)
        {
            await Task.Delay(100);
            Canvas.InvalidateVisual();
        }
    }


    private void CloseApp(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void StartDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
        NormalizeWindow();
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        TitleText.Text = Properties.Settings.Default.Title;
        Height = Properties.Settings.Default.Height;
        Width = Properties.Settings.Default.Width;
        if (Properties.Settings.Default.Top != 0 && Properties.Settings.Default.Left != 0)
        {
            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Properties.Settings.Default.Title = TitleText.Text;
        Properties.Settings.Default.Top = Top;
        Properties.Settings.Default.Left = Left;
        Properties.Settings.Default.Height = Height;
        Properties.Settings.Default.Width = Width;
        Properties.Settings.Default.Save();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        NormalizeWindow();
    }

    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        if (!IsActive && !renderBlank)
        {
            using var bitmap = new Bitmap((int)(Width * dpiX), (int)(Height * dpiY), PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen((int)(Left * dpiX), (int)(Top * dpiY), 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

            using var skBitmap = bitmap.ToSKBitmap();

            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(skBitmap, new SKPoint(0, 0));
        }
        else
        {
            canvas.Clear(SKColors.Transparent);
        }

        renderBlank = false;
    }


    private void NormalizeWindow()
    {
        Left = (int)(Left * dpiX) / dpiX;
        Top = (int)(Top * dpiY) / dpiY;

        Width = (int)(Width * dpiX) / dpiX;
        Height = (int)(Height * dpiY) / dpiY;
    }
}
