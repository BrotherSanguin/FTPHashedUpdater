/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11/07/2015
 * Time: 13:39
 * 
 */
using System;
using System.IO;
using Newtonsoft.Json;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public class Settings
	{
		private const String SettingsFile = @"config\settings.json";
		private JsonSerializerSettings _jsonsettings;
		
		public String FolderToUpdate{ get; set; }
		
		public String HashFileToCheck{ get; set; }
		
		public String FTPServerAdress{ get; set; }
		
		public String FTPUser{ get; set; }
		
		public String FTPPass{ get; set; }
		
		public static Settings Instance{ get; private set; }
		
		static Settings()
		{
			Instance = new Settings();
		}
		
		public Settings()
		{
			FolderToUpdate = String.Empty;
			HashFileToCheck = String.Empty;
			FTPServerAdress = String.Empty;
			
			_jsonsettings = new JsonSerializerSettings();
			_jsonsettings.TypeNameHandling = TypeNameHandling.All;//Best way to make sure there are no type-problems later
			_jsonsettings.Formatting = Formatting.Indented;
		}
		
		public static void Save()
		{
			Instance.SaveImpl();
		}
		
		public static void Load()
		{
			Instance.LoadImpl();
		}
		
		private void SaveImpl()
		{
			var spath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, SettingsFile);
			try {
				var data = JsonConvert.SerializeObject(this, _jsonsettings);
				if (File.Exists(spath))
					File.Delete(spath);
				File.WriteAllText(spath, data);
			} catch (Exception ex) {
        LogToFile.LogError("Error saving SettingsFile[{0}]:{1}", spath, ex);
			}
		}
		
		private void LoadImpl()
		{
			var spath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, SettingsFile);
			try {
        Console.WriteLine("Opening SettingsFile[" + spath + "]");
				if (File.Exists(spath)) {
					var data = File.ReadAllText(spath);
					var obj = JsonConvert.DeserializeObject(data, _jsonsettings);
					CopyOntoSelf(obj as Settings);
				} else
          LogToFile.LogWarn("SettingsFile [{0}] does not exist!", spath);
			} catch (Exception ex) {
        LogToFile.LogError("Error loading SettingsFile[{0}]: {1}", spath, ex);
			}
		}
		
		private void CopyOntoSelf(Settings othr)
		{
			//Reflection damit nicht jedes mal wenn eine neue Property dazukommt diese hier eintragen zu müssen.
			var type = GetType();
			var props = type.GetProperties();
			foreach (var prop in props) {
				var value = prop.GetValue(othr, null);
				prop.SetValue(this, value, null);
			}
		}
	}
}
