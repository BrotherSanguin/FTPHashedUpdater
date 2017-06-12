using System;
using System.Windows;

namespace FTPHashedUpdater
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      LogToFile.LogDebug("App started");
      Settings.Load();
      base.OnStartup(e);
      AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;
    }

    void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      LogToFile.LogError("CRASH:", e.ExceptionObject as Exception);
      LogToFile.Close();
    }
    protected override void OnExit(ExitEventArgs e)
    {
      Settings.Save();
      LogToFile.LogDebug("App exiting");
      LogToFile.Close();
      base.OnExit(e);
    }
  }
}