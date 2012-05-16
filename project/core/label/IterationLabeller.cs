using System;
using System.Text;
using System.Text.RegularExpressions;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.Core.Label
{
	/// <summary>
    /// The Iteration Labeller is similar to the <link>Default Labeller</link>; however, it maintains a revision number that is incremented by
    /// one for each iteration from the release start date. For example, if the release start date was June 1, 2005 and the iteration duration
    /// was 2 weeks, the iteration number on July 1, 2005 would be 3. This would create a label of &lt;prefix&gt;.3.&lt;build number&gt;.
	/// </summary>
    /// <title>Iteration Labeller</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;labeller type="iterationlabeller"&gt;
    /// &lt;prefix&gt;1.2&lt;/prefix&gt;
    /// &lt;duration&gt;1&lt;/duration&gt;
    /// &lt;releaseStartDate&gt;2005/6/24&lt;/releaseStartDate&gt;
    /// &lt;separator&gt;_&lt;/separator&gt;
    /// &lt;/labeller&gt;	
    /// </code>
    /// </example>
    /// <remarks>
    /// Contributed by Craig Campbell.
    /// </remarks>
	[ReflectorType("iterationlabeller")]
	public class IterationLabeller 
        : DefaultLabeller
	{
		private readonly DateTimeProvider dateTimeProvider;
		public const int InitialLabel = 1;
		private const int DaysInWeek = 7;

		public IterationLabeller() : this(new DateTimeProvider())
		{}

		public IterationLabeller(DateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		/// <summary>
        /// The duration of the iteration in weeks.
		/// </summary>
        /// <version>1.0</version>
        /// <default>2</default>
		[ReflectorProperty("duration", Required=false)]
		public int Duration = 2;

        /// <summary>
        /// The start date for the release (the start date of iteration one).
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
		[ReflectorProperty("releaseStartDate")]
		public DateTime ReleaseStartDate;

        /// <summary>
        /// The separator between the iteration number and the build number.
        /// </summary>
        /// <version>1.0</version>
        /// <default>.</default>
		[ReflectorProperty("separator", Required=false)]
		public string Separator = ".";

		public override string Generate(IIntegrationResult integrationResult)
		{
			IntegrationSummary lastIntegration = integrationResult.LastIntegration;
			if (lastIntegration.Label == null || lastIntegration.IsInitial())
			{
				return NewLabel(InitialLabel);
			}
			else if (lastIntegration.Status == IntegrationStatus.Success || IncrementOnFailed)
			{
				return NewLabel(IncrementLabel(lastIntegration.Label));
			}
			else
			{
				return lastIntegration.Label;
			}
		}

		private string NewLabel(int suffix)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append(LabelPrefix);
			if (LabelPrefix != string.Empty && ! LabelPrefix.EndsWith(Separator)) buffer.Append(Separator);
			buffer.Append(CurrentIteration());
			buffer.Append(Separator);
			buffer.Append(suffix);
			return buffer.ToString();
		}

		private int IncrementLabel(string label)
		{
			string iterationPtn = @".*?((\d+)" + Separator.Replace(".", @"\.") + "(\\d+$)).*";
			string iterationLabel = Regex.Replace(label, iterationPtn, "$2");
			int numericIteration = int.Parse(iterationLabel);
			if (numericIteration < CurrentIteration())
			{
				return InitialLabel;
			}
			else
			{
				string numericLabel = Regex.Replace(label, @".*?(\d+$)", "$1");
				int newLabel = int.Parse(numericLabel);
				return newLabel + 1;
			}
		}

		private int CurrentIteration()
		{
			return GetIteration(ReleaseStartDate);
		}

		private int GetIteration(DateTime startDate)
		{
			return GetIteration(startDate, dateTimeProvider.Today);
		}

		private int GetIteration(DateTime startDate, DateTime endDate)
		{
			double daysFromStart = (endDate - startDate).TotalDays;
			return (int) daysFromStart/(Duration*DaysInWeek);
		}
	}
}