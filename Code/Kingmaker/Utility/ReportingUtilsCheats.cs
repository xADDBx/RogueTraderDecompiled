using Core.Cheats;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility;

public class ReportingUtilsCheats
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	[Cheat(Name = "resend_reports")]
	public static void ResendReports(ReportSendingMode mode)
	{
		ReportingUtils.Instance.SetMode(mode);
	}
}
