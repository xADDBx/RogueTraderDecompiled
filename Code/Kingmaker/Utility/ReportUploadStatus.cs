using Newtonsoft.Json;

namespace Kingmaker.Utility;

public class ReportUploadStatus
{
	[JsonProperty("success")]
	public bool Success { get; set; }

	[JsonProperty("error")]
	public string Error { get; set; }

	[JsonProperty("reportid")]
	public string ReportId { get; set; }

	[JsonProperty("original_file_name")]
	public string OriginalFileName { get; set; }

	public override string ToString()
	{
		return string.Format("{0}(Success: {1}, Error: {2}, ReportId: {3}, FileName: {4})", "ReportUploadStatus", Success, Error, ReportId, OriginalFileName);
	}
}
