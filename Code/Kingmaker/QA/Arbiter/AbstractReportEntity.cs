using System.Xml;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
internal abstract class AbstractReportEntity
{
	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string ExternalLogId { get; set; }

	public virtual void WriteTestCaseToXml(XmlWriter xmlWriter)
	{
	}
}
