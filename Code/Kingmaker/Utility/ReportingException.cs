using System;
using System.Collections.Generic;

namespace Kingmaker.Utility;

public class ReportingException : Exception
{
	public ReportingUtils.FixVersions TargetVersion { get; set; }

	public ReportingUtils.Severity SuggestionSeverity { get; set; }

	public IList<string> AdditionalLabels { get; set; } = new List<string>();


	protected ReportingException(ReportingUtils.Severity suggestedSeverity, ReportingUtils.FixVersions targetVersion, IEnumerable<string> additionalLabels)
	{
		SuggestionSeverity = suggestedSeverity;
		TargetVersion = targetVersion;
		if (additionalLabels == null)
		{
			return;
		}
		foreach (string additionalLabel in additionalLabels)
		{
			AdditionalLabels.Add(additionalLabel);
		}
	}
}
