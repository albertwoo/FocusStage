using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace FocusStage;

public partial class MainWindow : Window
{
    private readonly Matrix transformToDevice;

    public MainWindow()
    {
        InitializeComponent();

        Closed += MainWindow_Closed;

        Height = Properties.Settings.Default.Height;
        Width = Properties.Settings.Default.Width;
        if (Properties.Settings.Default.Top != 0 && Properties.Settings.Default.Left != 0)
        {
            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
        }

        windowChrome.ResizeBorderThickness = new Thickness(3, 0, 3, 3);
        windowChrome.CaptionHeight = 0;

        using var source = new HwndSource(new HwndSourceParameters());
        transformToDevice = source.CompositionTarget.TransformToDevice;

        RunUpdateImageTask();
    }

    private async Task RunUpdateImageTask()
    {
        while (true)
        {
            await Task.Delay(100);

            if (!IsActive)
            {
                await UpdateImage();
            }
            else
            {
                Image.Source = null;
            }
        }
    }

    private async Task UpdateImage()
    {
        var widthAndHeight = transformToDevice.Transform(new Vector(Width, Height));
        var leftAndTop = transformToDevice.Transform(new Vector(Left, Top));

        using var bitmap = new Bitmap((int)widthAndHeight.X, (int)widthAndHeight.Y, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);

        Image.Source = null;
        await Task.Delay(5);

        graphics.CopyFromScreen((int)leftAndTop.X, (int)leftAndTop.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

        Image.Source = BitmapToImageSource(bitmap);
    }

    private static BitmapImage BitmapToImageSource(Image bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        memory.Position = 0;
        var bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();
        return bitmapimage;
    }

    private void CloseApp(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void StartDrag(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        Properties.Settings.Default.Top = Top;
        Properties.Settings.Default.Left = Left;
        Properties.Settings.Default.Height = Height;
        Properties.Settings.Default.Width = Width;
        Properties.Settings.Default.Save();
    }
}
