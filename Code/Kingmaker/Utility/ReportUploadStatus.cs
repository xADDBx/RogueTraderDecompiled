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
}
