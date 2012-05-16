
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThoughtWorks.CruiseControl.Core.Util
{
	/// <summary>
	///  This timer class is useful for diagnosing timing issues.  Just create an instance in a using statement
	/// to trace out how long things take:
	/// 
	/// using (new AccurateTimer("Some activity"))
	/// {
	///		// stuff to be timed
	/// }
	/// </summary>
	public class AccurateTimer : IDisposable
	{
		private static readonly long frequency;

		private long startTime;
		private string activityName;

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

		static AccurateTimer()
		{
			QueryPerformanceFrequency(out frequency);
		}

		public AccurateTimer(string activityName)
		{
			this.activityName = activityName;
			Start();
		}

		private void Start()
		{
			Trace.WriteLine(string.Format("Start: {0}", activityName));
			QueryPerformanceCounter(out startTime);
		}

		private void End()
		{
			long endTime;
			QueryPerformanceCounter(out endTime);

			long durationInMs = ((endTime - startTime)*1000)/frequency;

			Trace.WriteLine(string.Format("Done: {0} took {1} ms", activityName, durationInMs));
		}

		public void Dispose()
		{
			End();
		}
	}

}
