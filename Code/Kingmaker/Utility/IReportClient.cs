using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Utility.Reporting.Base;

namespace Kingmaker.Utility;

public interface IReportClient
{
	Task<ReportUploadStatus> UploadAsync(string reportFileName, ReportSendParameters reportSendParameters, CancellationToken token);
}
