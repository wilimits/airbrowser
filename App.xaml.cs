using System.Configuration;
using System.Data;
using System.Windows;

namespace SearchBrowser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        
        string dir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "AirBrowser");
        string marker = System.IO.Path.Combine(dir, "firstrun.done");
        
        if (!System.IO.File.Exists(marker))
        {
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            
            var splash = new WelcomeWindow();
            splash.ShowDialog();
            
            try
            {
                System.IO.File.WriteAllText(marker, "done");
            }
            catch { }
        }
        
        base.OnStartup(e);
        this.ShutdownMode = ShutdownMode.OnLastWindowClose;
    }
}

