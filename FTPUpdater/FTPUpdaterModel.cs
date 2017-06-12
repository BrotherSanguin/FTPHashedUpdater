/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 10/11/2015
 * Time: 17:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

using System.Windows.Forms;
using System.Windows.Input;


namespace FTPHashedUpdater
{
  /// <summary>
  /// Model assync Update of a defined Folder
  /// </summary>
  public class FTPUpdaterModel : AbstractNotify
  {
    private Settings _settings;
    private FTPConnection _ftp;
    private Regex _hasFileRegex = new Regex("\\\"([^\\\"]*)\\\"\\s+([^\\s]*)", RegexOptions.Singleline | RegexOptions.Compiled);
    private const string TEMP = "Temp";
    private ThreadWrap _loadThread;
    private volatile bool _isUpdating;

    public IList<LogEntryDO> LogEntries { get; private set; }

    public bool IsUpdating
    {
      get { return _isUpdating; }
      set
      {
        if (!Equals(_isUpdating, value))
        {
          _isUpdating = value;
          RaisePropertyChanged(() => IsUpdating);
        }
      }
    }

    public string FolderToUpdate
    {
      get { return _settings.FolderToUpdate; }
      set
      {
        if (!Equals(_settings.FolderToUpdate, value))
        {
          _settings.FolderToUpdate = value;
          RaisePropertyChanged(() => FolderToUpdate);
        }
      }
    }

    public String FTPAdress
    {
      get { return _settings.FTPServerAdress; }
      set
      {
        if (!Equals(_settings.FTPServerAdress, value))
        {
          _settings.FTPServerAdress = value;
          RaisePropertyChanged(() => FTPAdress);
        }
      }
    }

    public String FTPUser
    {
      get { return _settings.FTPUser; }
      set
      {
        if (!Equals(_settings.FTPUser, value))
        {
          _settings.FTPUser = value;
          RaisePropertyChanged(() => FTPUser);
        }
      }
    }

    public String FTPPass
    {
      get { return _settings.FTPPass; }
      set
      {
        if (!Equals(_settings.FTPPass, value))
        {
          _settings.FTPPass = value;
          RaisePropertyChanged(() => FTPPass);
        }
      }
    }

    public ProgressDO FileProgress
    {
      get;
      private set;
    }

    public ProgressDO GlobalProgress
    {
      get;
      private set;
    }

    public ICommand DoUpdateCommand { get; private set; }

    public ICommand DoBrowseCommand { get; private set; }

    public FTPUpdaterModel()
    {
      _loadThread = new ThreadWrap(DoUpdate, "FTPUpdater.LoadThread", false);
      _loadThread.ThreadExceptionOccurred += _loadThread_ThreadExceptionOccurred;
      LogEntries = new ObservableCollection<LogEntryDO>();

      FileProgress = new ProgressDO();
      GlobalProgress = new ProgressDO();

      DoUpdateCommand = new RelayCommand(x => StartUpdate(), x => CanUpdate());
      DoBrowseCommand = new RelayCommand(x => BrowseTargetFolder());
      _settings = Settings.Instance;
      _ftp = new FTPConnection(_settings.FTPServerAdress, _settings.FTPUser, _settings.FTPPass);
    }

    void _loadThread_ThreadExceptionOccurred(object sender, ThreadExceptionEventArgs e)
    {
      Log(LogMode.Error, "Update-Thread-Crash! " + e.Exception);
      LogToFile.LogError("Update-Thread-Exception:{0}", e.Exception);
      IsUpdating = false;
    }
    private bool CanUpdate()
    {
      return !_loadThread.IsAlive;
    }

    private void StartUpdate()
    {
      _loadThread.Start();
    }

    private void DoUpdate()
    {
      IsUpdating = true;
      GlobalProgress.Progress = 0;
      FileProgress.Progress = 0;
      var bdir = System.AppDomain.CurrentDomain.BaseDirectory;
      var temp = Path.Combine(bdir, TEMP);
      var hashfilelocal = Path.Combine(_settings.FolderToUpdate, _settings.HashFileToCheck);
      IDictionary<string, string> hashdictlocal;
      if (!File.Exists(hashfilelocal))
      {
        Log(LogMode.Waring, "Lokale Hashdatei " + hashfilelocal + " nicht vorhanden. Alles wird geladen.");
        hashdictlocal = new Dictionary<string, string>();
      }
      else {
        Log(LogMode.Debug, "Lade lokale Hashdatei " + hashfilelocal + ".");
        hashdictlocal = ReadHashFile(hashfilelocal);
      }
      var downloadlist = new List<String>();
      if (!DownloadFile(_settings.HashFileToCheck))
      {
        Log(LogMode.Error, "HashDatei [" + _settings.HashFileToCheck + "] konnte nicht vom Server geladen werden");
        return;
      }

      var downhashfile = Path.Combine(Path.Combine(bdir, TEMP), _settings.HashFileToCheck);
      Log(LogMode.Debug, "Lade remote Hashdatei " + downhashfile + ".");
      var downhashdict = ReadHashFile(downhashfile);

      foreach (var key in downhashdict.Keys)
      {
        if (!hashdictlocal.ContainsKey(key))
        {
          Log(LogMode.Debug, "Datei " + key + " fehlt im lokalen Hashfile. Vorgemerkt zum Laden.");
          downloadlist.Add(key);
        }
        else if (!Equals(downhashdict[key], hashdictlocal[key]))
        {
          Log(LogMode.Debug, "Datei " + key + " Hash ungleich. Vorgemerkt zum Laden.");
          downloadlist.Add(key);
        }
      }
      if (downloadlist.Count == 0)
      {
        Log(LogMode.Debug, "Kein download nötig, alle Dateien aktuell");
        return;
      }

      GlobalProgress.TotlalProgress = GetTotalByteSizeOfRemoteFiles(downloadlist);
      try
      {
        Log(LogMode.Debug, "Starte downloads...");
        foreach (var todownload in downloadlist)
        {
          if (!DownloadFile(todownload))
          {
            //Wenn ein Download Fehlschlägt, beenden!
            Log(LogMode.Error, "HashDatei [" + _settings.HashFileToCheck + "] konnte nicht vom Server geladen werden");
            Directory.Delete(temp, true);
            return;
          }


        }
      }
      catch (Exception ex)
      {
        Log(LogMode.Error, ex.ToString());
        Directory.Delete(temp, true);//Bei gröberen Fehler ebenfalls benden und löschen der evtl. schon geladenen teile
        return;
      }
      //Gedownloadete Dateien aus dem Temp-Verzeichnis in das Aktuelle verschieben
      Log(LogMode.Debug, "Downloads fertig, verschiebe Daten aus dem Temp-Verzeichnis nach " + _settings.FolderToUpdate);
      foreach (var downloaded in downloadlist)
      {
        var realfile =
          downloaded.StartsWith(Path.DirectorySeparatorChar + "", StringComparison.InvariantCulture) ?
          downloaded.Remove(0, 1) : downloaded;
        var tmpfile = Path.Combine(temp, realfile);
        var targetfile = Path.Combine(_settings.FolderToUpdate, downloaded);
        Log(LogMode.Debug, "Verschiebe [" + downloaded + "]");
        CreateDirectoryIfNotExists(targetfile);
        File.Copy(tmpfile, targetfile, true);
        File.Delete(tmpfile);
      }
      //Da Hashfile nicht teil der DownloadListe war muss es hier nochmal extra behandelt werden
      var hashfile = Path.Combine(_settings.FolderToUpdate, _settings.HashFileToCheck);
      Log(LogMode.Debug, "Verschiebe [" + _settings.HashFileToCheck + "]");
      CreateDirectoryIfNotExists(hashfile);
      File.Copy(downhashfile, hashfile, true);
      File.Delete(downhashfile);
      //Temp-Dir löschen
      Directory.Delete(temp, true);
      Log(LogMode.Debug, "Verschieben erfolgreich Update Fertig!");
      IsUpdating = false;
    }

    private void BrowseTargetFolder()
    {
      using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
      {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
          DialogResult result = dialog.ShowDialog();
          if (result == DialogResult.OK)
            FolderToUpdate = dialog.SelectedPath;
        });
      }

    }


    private long GetTotalByteSizeOfRemoteFiles(IList<string> filelist)
    {
      long total = 0;
      foreach (var file in filelist)
        total += _ftp.GetFileSize(file);
      return total;
    }

    private bool DownloadFile(string name)
    {
      FileProgress.Progress = 0;
      FileProgress.ProgressText = name;
      var temppath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, TEMP);
      if (!Directory.Exists(temppath))
        Directory.CreateDirectory(temppath);
      if (name.StartsWith(Path.DirectorySeparatorChar + "", StringComparison.InvariantCulture))
        name = name.Remove(0, 1);
      var fpath = Path.Combine(temppath, name);
      if (File.Exists(fpath))
        File.Delete(fpath);
      CreateDirectoryIfNotExists(fpath);
      Log(LogMode.Debug, "Downloading " + name);
      _ftp.DownloadFile(name, fpath, FileProgress, GlobalProgress);
      return File.Exists(fpath);
    }

    private void CreateDirectoryIfNotExists(string path)
    {
      var dir = Path.GetDirectoryName(path);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
    }

    private IDictionary<string, string> ReadHashFile(string hashfile)
    {
      var data = File.ReadAllText(hashfile);
      var dict = new Dictionary<String, String>();
      var matches = _hasFileRegex.Matches(data);
      for (int i = 0; i < matches.Count; i++)
      {
        var match = matches[i];
        var filename = match.Groups[1].Value;
        var b64hash = match.Groups[2].Value;
        dict.Add(filename, b64hash);
      }
      return dict;
    }

    private void Log(LogMode mode, String logentry)
    {
      LogToFile.Log(mode, logentry);

      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        if (LogEntries.Count >= 100)
          LogEntries.RemoveAt(LogEntries.Count - 1);
        LogEntries.Insert(0, new LogEntryDO(mode, logentry));
      });
    }
  }

  public enum LogMode
  {
    Debug,
    Waring,
    Error
  }
}
