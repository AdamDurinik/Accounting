using System.IO;
using System.Windows;
using System.Windows.Threading;
using Velopack;
using MessageBox = System.Windows.MessageBox;

namespace PriceTags
{
    public partial class App : System.Windows.Application
    {
        public void OnStartup(object sender, StartupEventArgs e)
        {
            VelopackApp.Build().Run();
            RegisterGlobalExceptionHandlers();
            try
            {
                var viewModel = new ViewModels.MainViewModel();
                var mainWindow = new MainWindow(viewModel);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                HandleException(ex, "Unexpected error during application startup.");
                try
                {
                    Shutdown(-1);
                }
                catch
                {
                }
            }
        }

        private void RegisterGlobalExceptionHandlers()
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "Nečakaný error nastal, asi z UI, pravdepodobne devexpress. \n Napíš Adamovy...\n\n"+e.Exception.Message);
            }
            catch
            {
                // Swallow
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void OnDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception ?? new Exception("Non-Exception domain error.");
                HandleException(ex, "An unexpected non-UI error occurred.");
            }
            catch
            {
                // Swallow
            }

            if (!e.IsTerminating) return;
            try
            {
                Current?.Dispatcher?.InvokeShutdown();
            }
            catch
            {
                // Swallow
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "An unobserved task exception occurred.");
            }
            catch
            {
                // Swallow
            }
            finally
            {
                e.SetObserved();
            }
        }

        private void HandleException(Exception ex, string context)
        {
            var timestamp = DateTimeOffset.Now;
            var logText = $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] {context}{Environment.NewLine}{ex}{Environment.NewLine}";

            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var logDir = Path.Combine(localAppData, "PriceTags", "Logs");
                Directory.CreateDirectory(logDir);

                var fileName = $"error-{timestamp:yyyyMMdd-HHmmss-fff}.log";
                var filePath = Path.Combine(logDir, fileName);

                File.WriteAllText(filePath, logText);
            }
            catch
            {
                // If logging fails, do not throw; fall back to best-effort display.
            }

            try
            {
                void show()
                {
                    var message = $"{context}{Environment.NewLine}An error occurred and was logged. Please restart the application.";
                    MessageBox.Show(message, "PriceTags - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (Dispatcher is not null && Dispatcher.CheckAccess())
                {
                    show();
                }
                else if (Dispatcher is not null)
                {
                    Dispatcher.Invoke(show);
                }
                // No dispatcher available; avoid throwing.
            }
            catch
            {
                // Swallow any exceptions thrown while attempting to show UI.
            }
        }
    }
}
