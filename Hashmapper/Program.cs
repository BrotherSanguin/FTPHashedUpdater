/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11.10.2015
 * Time: 08:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading;


namespace Hashmapper
{
	/// <summary>
	/// The Hashmapper should recursively work through any given Folder(Parameter) and hash all files found there.
	/// The hashes should be saved in a textfile with their relative filepaths.
	/// </summary>
	class Program
	{
		
		private static MD5 _hasher;
		private static ConsoleColor _okcolor = ConsoleColor.Green;
		private static ConsoleColor _errColor = ConsoleColor.Red;
		private static ConsoleColor _wrnColor = ConsoleColor.Yellow;
		private static string _mappingfile;
		private static bool _debugmode = false;
		
		/// <summary>
		/// Hashmapper expects 2 Parameters
		/// 1. target-folder
		/// 2. target-textfile for the calculated hashes
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			try {
				Console.WriteLine("Ex's Hashmapper 1.0");
				Console.Write("Validating parameters...");
				if (args.Length < 2) {
					WriteConsole(_errColor,Environment.NewLine + "There are parameters missing!");
					PrintHelp();
					return;
				}
				var targetpath = args[0];
				_mappingfile = Path.GetFullPath(args[1]);
				_debugmode = args.Length > 2 && args[2].Equals("-d");
				WriteConsole(_okcolor, "OK");
				Console.Write("Initializing ...");
				IDictionary<string,string> hashdict = new Dictionary<string,string>();
				_hasher = MD5.Create();//Hasher muss initialisiert werden bevor gemappt wird!
				WriteConsole(_okcolor, "OK");
				Console.Write("Looking up full sourcepath...");
				var fullstartpath = Path.GetFullPath(targetpath);//Pfad vervollständigen falls es sich um einen relativen Pfad handelt
				WriteConsole(_okcolor, "OK");
				Console.WriteLine("Creating hashes from Folder {0} ...", targetpath);
				MapFiles(fullstartpath, fullstartpath, hashdict);
				WriteConsole(_okcolor, "All hashes OK");
				Console.Write("Writting hashes to file {0} ...", _mappingfile);
				WriteHashDictToFile(_mappingfile, hashdict);
				WriteConsole(_okcolor, "OK");
				GoCrazy();
			
			} catch (Exception ex) {
				WriteConsole(_errColor, "ERROR:" + ex.Message);
				WriteExceptionLog(Path.GetFullPath("Exception.log"), ex);
				PrintHelp();
				if (!_debugmode)//Wenn der Debugmodus aus ist...
					Thread.Sleep(2000);//2 Sekunden zum lesen der Fehler
			}
			if (_debugmode) {
				Console.Write("END -> Press any key to close programm");
				Console.ReadKey(true);
			}
		}
		
		/// <summary>
		/// Schreibt HILFE-Infos in die Console
		/// </summary>
		private static void PrintHelp()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Usage: Hashmapper <TargetFolderPath> <MapFilePath> [-d]").Append(Environment.NewLine);
			sb.Append("TargetFolerPath = Path to targetfolder(relative paths are supported)").Append(Environment.NewLine);
			sb.Append("MapFilePath = path(incl. filename) of the target-textfile").Append(Environment.NewLine);
			sb.Append("-d = Debugmode(programm does not end without userinteraction)");
			WriteConsole(ConsoleColor.Magenta, sb.ToString());
		}
		
		/// <summary>
		/// Rekursive Methode, die den Inhalt des derzeitigen Ordners in das Hashdict mappt und anschließend
		/// das selbe für die Unterordner macht.
		/// ACHTUNG: startuppath wird gebraucht um den Absoluten Pfad der Dateien auf eine relative Länge zu kürzen!
		/// </summary>
		/// <param name="staruppath">Starpfad(ändert sich nie)</param>
		/// <param name="currentfolder">Derzeitiger Ordner</param>
		/// <param name="hashdict">Hashdict zum zwischenspeicher der Rel-Pfad/Hash- Combo</param>
		private static void MapFiles(string staruppath, string currentfolder, IDictionary<string,string> hashdict)
		{
			var foldershort = currentfolder.Remove(0, staruppath.Length);
			foldershort = (String.IsNullOrEmpty(foldershort) ? @"\" : foldershort);
			Console.WriteLine("Reading Folder {0} ->", foldershort);
			var files = Directory.GetFiles(currentfolder);
			foreach (var file in files) {
				var relpath = file.Remove(0, staruppath.Length);//Kürzen des absoluten Pfades zu Relativem Pfad
				Console.Write("{0} ...", relpath);
				if (Equals(_mappingfile, file)) {
					WriteConsole(_wrnColor, "is MappingFile --> SKIPPED");
					continue;
				}
				
				var hash = GetBase64_MD5_Hash(file);
				hashdict.Add(relpath, hash);
				WriteConsole(_okcolor, "OK");
			}
			var subdirectories = Directory.GetDirectories(currentfolder);
			foreach (var dir in subdirectories)
				MapFiles(staruppath, dir, hashdict);
		}
		
		/// <summary>
		/// Berechnet den MD5-Hash einer Datei und gibt ihn als Base64 String zurück
		/// </summary>
		/// <param name="file">Datei</param>
		/// <returns></returns>
		private static string GetBase64_MD5_Hash(string file)
		{
			var bytes = File.ReadAllBytes(file);
			var hash = _hasher.ComputeHash(bytes);
			var base64 = Convert.ToBase64String(hash);
			return base64;
		}
		
		/// <summary>
		/// Schreibt das errechnete Hashdict in eine Datei
		/// Dabei wird der Pfad in mit Appostrophen vershen, da sich darin Leerzeichen befinden können und
		/// sonst keine (einfache)Möglichkeit besteht zwischen Pfad und Hash zu trennen.
		/// </summary>
		/// <param name="filename">Datei</param>
		/// <param name="hashdict">Hashdict</param>
		private static void WriteHashDictToFile(string filename, IDictionary<string,string> hashdict)
		{
			using (var fs = File.OpenWrite(filename)) {
				using (StreamWriter sw = new StreamWriter(fs)) {
					foreach (var havp in hashdict) {
						sw.WriteLine("\"" + havp.Key + "\" " + havp.Value);
						sw.Flush();//Verursacht ein schreiben des Puffers in die Datei nach jeder Zeile 
					}
				}
			}
		}
		
		/// <summary>
		/// Hilfs-Methode um einfacher Farbigen Text in der Cosole ausgeben zu könnnen.
		/// Farbe wird beim beenden der Methode zurückgesetzt.
		/// </summary>
		/// <param name="color">Schrift-Farbe</param>
		/// <param name="text">Text</param>
		/// <param name="nl">Neue Zeile(Ja/Nein)</param>
		/// <param name = "background">Hintergrudfarbe</param>
		private static void WriteConsole(ConsoleColor color, string text, bool nl = true, ConsoleColor background = ConsoleColor.Black)
		{
			Console.ForegroundColor = color;
			Console.BackgroundColor = background;
			Console.Write(text + (nl ? Environment.NewLine : ""));
			Console.ResetColor();
		}
		
		/// <summary>
		/// Schreibt Exceptions(Fehler) in eine Datei, damit ggf. die Ursache ausgemacht werden kann.
		/// ACHTUNG: Der Einfachheit halber überschreibt sich die Datei bei jedem Fehler!
		/// Soll heißen Fehler-Bericht sollte vor dem nächsten Veruch weggesichert werden.
		/// </summary>
		/// <param name="file">Fehlerbericht</param>
		/// <param name="ex">Fehler</param>
		private static void WriteExceptionLog(string file, Exception ex)
		{
			try {
				WriteConsole(_wrnColor, "Writting Error-Log " + file + " ...", false);
				using (var fs = File.OpenWrite(file)) {
					using (StreamWriter sw = new StreamWriter(fs)) {
						DateTime now = DateTime.Now;
						sw.WriteLine("SNAFU");
						sw.WriteLine(now.ToLongDateString() + " - " + now.ToLongTimeString());
						while (ex != null) {
							WriteExceptionInfosToStreamWriter(sw, ex);
							ex = ex.InnerException;
						}
					}
				}
				WriteConsole(_okcolor, "OK");
			} catch (Exception wtf) {//Wenn beim schreiben des Fehlerberichts ein Fehler auftritt soll dieser auf keinen Fall den Originalfehler verdecken!!!
				WriteConsole(_errColor, "ERROR: " + file + ":" + wtf.Message);
			}
			
		}
		
		/// <summary>
		/// Schreibe Fehler infos in einen Stream(--> sinnvolerweise eine Datei)
		/// </summary>
		/// <param name="sw">Stream-Schreiber</param>
		/// <param name="ex">Fehler</param>
		private static void WriteExceptionInfosToStreamWriter(StreamWriter sw, Exception ex)
		{
			sw.WriteLine("Exception.Message: " + ex.Message);
			sw.WriteLine("typeof(Exception): " + ex.GetType());
			;
			sw.WriteLine("Exception.Stacktrace: " + ex.StackTrace);
		}
		
		private static void GoCrazy()
		{
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.Green, "F", false);
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.DarkGreen, "I", false);
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.Magenta, "N", false);
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.Blue, "I", false);
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.DarkBlue, "S", false);
			Thread.Sleep(400);
			WriteConsole(ConsoleColor.White, "H", false);
			Thread.Sleep(400);
      WriteConsole(ConsoleColor.Green, "E", false);
      Thread.Sleep(400);
      WriteConsole(ConsoleColor.DarkGreen, "D", false);
      Thread.Sleep(400);
		}
	}
}