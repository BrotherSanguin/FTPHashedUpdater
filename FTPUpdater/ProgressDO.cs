/*
 * Created by SharpDevelop.
 * User: Ex
 * Date: 11/07/2015
 * Time: 15:30
 * 
 */
using System;

namespace FTPHashedUpdater
{
	/// <summary>
	/// Description of ProgressDO.
	/// </summary>
	public class ProgressDO : AbstractNotify
	{
		private long _progress;
		private long _totalprogress;
		private double _percent;
		private string _progressText;
		
		public long TotlalProgress {
			get{ return _totalprogress; }
			set {
				if (!Equals(_totalprogress, value)) {
					_totalprogress = value;
					UdpatePercent();
					RaisePropertyChanged(() => TotlalProgress);
				}
			}
		}
		
		public long Progress {
			get{ return _progress; }
			set {
				if (!Equals(_progress, value)) {
					_progress = value;
					UdpatePercent();
					RaisePropertyChanged(() => Progress);
				}
			}
		}
		
		public double Percent {
			get{ return _percent; }
			set {
				if (!Equals(_percent, value)) {
					_percent = value;
					RaisePropertyChanged(() => Percent);
				}
			}
		}
		
		
		
		public string ProgressText {
			get{ return _progressText; }
			set {
				if (!Equals(_progressText, value)) {
					_progressText = value;
					RaisePropertyChanged(() => ProgressText);
				}
			}
		}
		
		public ProgressDO()
		{
			_progress = 0;
			_progressText = "";
		}
		
		private void UdpatePercent()
		{
			Percent = (_totalprogress == 0 ? 0 : (double)_progress / (double)_totalprogress * 100d);
		}
	}
}
