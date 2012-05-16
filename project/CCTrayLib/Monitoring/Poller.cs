using System.Diagnostics;
using System.Timers;

namespace ThoughtWorks.CruiseControl.CCTrayLib.Monitoring
{
	/// <summary>
	///  Polls an IPollable thing
	/// </summary>
	public class Poller
	{
		private Timer timer;
		private IPollable itemToPoll;

		public Poller(int pollIntervalSeconds, IPollable itemToPoll)
		{
			this.itemToPoll = itemToPoll;
			timer = new Timer(pollIntervalSeconds*1000);
			timer.AutoReset = false;
			timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
		}


		private void Timer_Elapsed(object args, ElapsedEventArgs e)
		{
			try
			{
				itemToPoll.OnPollStarting();
				Debug.WriteLine("Polling...");
				itemToPoll.Poll();
			}
			finally
			{
				timer.Start();
			}
		}

		public void Start()
		{
			timer.Stop();
			// Fire the timer straight away rather than waiting for poll interval
			Timer_Elapsed(null, null);
		}

		public void Stop()
		{
			timer.Stop();
		}
	}
}
