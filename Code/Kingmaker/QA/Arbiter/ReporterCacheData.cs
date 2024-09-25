using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
internal class ReporterCacheData
{
	[JsonProperty]
	public Guid JobGuid = Guid.NewGuid();

	[JsonProperty]
	public List<AbstractReportEntity> ReportEntities = new List<AbstractReportEntity>();
}
