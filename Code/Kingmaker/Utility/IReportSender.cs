using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Utility;

public interface IReportSender : ISubscriber
{
	void HandleReportSend(ReportSendEntry sendEntry);

	void HandleReportResultReceived(ReportSendEntry sendEntry);
}
