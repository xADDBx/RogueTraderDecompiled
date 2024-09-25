using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

internal abstract class AbstractAggregatedReportEntity : AbstractReportEntity
{
	[JsonProperty]
	public bool IsStarted { get; set; }

	[JsonProperty]
	public bool IsFromSkippedInstruction { get; set; }

	[JsonProperty]
	public bool IsFromFailedInstruction { get; set; }
}
