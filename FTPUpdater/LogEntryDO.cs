/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 08.11.2015
 * Time: 10:58
 * 
 */
using System;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Simple LogEntryDO for display in a WPF-Control.
	/// </summary>
	public class LogEntryDO
	{
		public String Message{get;private set;}
		
		public LogMode LogMode{get;private set;}
		
		public LogEntryDO(LogMode mode, String message)
		{
			Message = message;
			LogMode = mode;
		}
	}
}
