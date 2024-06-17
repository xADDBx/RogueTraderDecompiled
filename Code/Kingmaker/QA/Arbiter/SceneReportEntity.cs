using System.Xml;

namespace Kingmaker.QA.Arbiter;

internal class SceneReportEntity : AbstractAggregatedReportEntity
{
	public override void WriteTestCaseToXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("name", base.Name);
		if (!base.IsStarted)
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString("Scene never loaded");
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}
}
