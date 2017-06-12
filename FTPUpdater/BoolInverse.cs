/*
 * Created by SharpDevelop.
 * User: EX
 * Date: 11/14/2015
 * Time: 18:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Data;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Description of BoolInverse.
	/// </summary>
	public class BoolInverse : IValueConverter
	{
		#region IValueConverter implementation
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return !(bool)value;
	}
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
	#endregion
		
	}
}
