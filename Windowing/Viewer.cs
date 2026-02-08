using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Windowing;

public interface IWindowUpdate
{
    bool IsClosed { get; }
    void UpdateImage(ReadOnlySpan<byte> data);
    void UpdateStatus(string text);
}

public class Viewer
{
    public static void Show(int width, int height, string title, Action<IWindowUpdate> renderLoop)
    {
        App.Width = width;
        App.Height = height;
        App.Title = title;

        App.OnStartup = window =>
        {
            var updater = new WindowUpdater(window);
            Task.Run(() =>
            {
                try
                {
                    renderLoop(updater);
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(() => { updater.UpdateStatus($"Error: {ex.Message}"); });
                    Console.WriteLine($"Render Loop Crash: {ex}");
                }
            });
        };

        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(Array.Empty<string>(), ShutdownMode.OnMainWindowClose);
    }

    private class WindowUpdater : IWindowUpdate
    {
        private readonly MainWindow _win;

        public WindowUpdater(MainWindow win)
        {
            _win = win;
            _win.Closed += (s, e) => IsClosed = true;
        }

        public void UpdateImage(ReadOnlySpan<byte> data)
        {
            if (IsClosed) return;
            _win.UpdateImage(data);
        }

        public void UpdateStatus(string text)
        {
            if (IsClosed) return;
            _win.UpdateStatus(text);
        }

        public bool IsClosed { get; private set; }
    }
}