/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11/08/2015
 * Time: 11:12
 * 
 */
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Description of LogModeToSolidBrush.
	/// </summary>
	public class LogModeToSolidBrush : IValueConverter
	{
		#region IValueConverter implementation

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		LogMode mode = (LogMode)value;
		switch (mode) {
				case LogMode.Debug:
				value = Brushes.Green;
					break;
				case LogMode.Waring:
					value = Brushes.Yellow;
					break;
				case LogMode.Error:
					value = Brushes.Red;
					break;
			}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	#endregion

	}
}
