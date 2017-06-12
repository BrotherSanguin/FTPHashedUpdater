/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11/07/2015
 * Time: 15:31
 * 
 */
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Description of AbstractNotify.
	/// </summary>
	public class AbstractNotify : INotifyPropertyChanged
	{
		public bool RaiseWithDispatcher{get;protected set;}
		
		#region INotifyPropertyChanged implementation
		
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
		
		public static string GetPropertyName<P>(Expression<Func<P>> action)
		{
			MemberExpression expression = action.Body as MemberExpression;
			return expression.Member.Name;
		}
		
		protected void RaisePropertyChanged<P>(Expression<Func<P>> action){
			RaisePropertyChanged(GetPropertyName(action));
		}
		
		protected void RaisePropertyChanged(string property)
		{
			if(RaiseWithDispatcher)
				Application.Current.Dispatcher.Invoke(() => RaisePropertyChangedImpl(property));
			else
				RaisePropertyChangedImpl(property);
			
		}
		
		private void RaisePropertyChangedImpl(string property){
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
