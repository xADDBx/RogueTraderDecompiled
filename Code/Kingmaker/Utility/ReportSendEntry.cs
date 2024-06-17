using System;
using Kingmaker.Utility.Reporting.Base;

namespace Kingmaker.Utility;

public class ReportSendEntry
{
	public string Guid => SendParameters.Guid;

	public string ReportId { get; set; }

	public string ReportIssueId { get; set; }

	public int ReportIssueCheckRetryCount { get; set; }

	public DateTime FirstRetry { get; set; }

	public DateTime LastRetry { get; set; }

	public int SendRetryCount { get; set; }

	public ReportSendParameters SendParameters { get; }

	public string File { get; }

	public ReportSendEntry(string file, ReportSendParameters reportSendParameters)
	{
		File = file;
		SendParameters = reportSendParameters;
		FirstRetry = DateTime.Now;
		LastRetry = DateTime.Now;
		SendRetryCount = 0;
	}
}
