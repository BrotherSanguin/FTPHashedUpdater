using System;
using System.IO;

namespace FTPHashedUpdater
{
  public class LogToFile
  {
    public const string LogFileName = "FtpUpdater.log";

    private static Stream _source;
    private static StreamWriter _sw;
    private static bool _broken = false;

    static LogToFile()
    {
      try
      {
        _source = File.OpenWrite(LogFileName);
        _sw = new StreamWriter(_source);
      }
      catch (Exception ex)
      {
        _broken = true;
        Console.WriteLine("ERROR: Opening LogFile:" + ex);
        Close();
      }
    }

    public static void Log(LogMode mode, string data, params object[] args)
    {
      if (_broken)
        return;
      string msg = data;
      if(args != null && args.Length > 0)
        msg = string.Format(data, args);
      _sw.WriteLine("{0}\t{1}\t{2}",DateTime.Now.ToString("G"), mode, msg);
      _sw.Flush();
    }

    public static void LogDebug(string data, params object[] args)
    {
      Log(LogMode.Debug, data, args);
    }

    public static void LogWarn(string data, params object[] args)
    {
      Log(LogMode.Waring, data, args);
    }

    public static void LogError(string data, params object[] args)
    {
      Log(LogMode.Error, data, args);
    }

    public static void Close()
    {
      if (_sw != null)
        _sw.Close();
      _sw = null;
      if (_source != null)
        _source.Close();
      _source = null;
    }
  }
}
